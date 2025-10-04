using Microsoft.Extensions.Logging;

namespace PowerUnit.Common.TimeoutService;

internal sealed class TimeoutService : ITimeoutService, IDisposable
{
    private readonly ILogger<TimeoutService> _logger;

    private const int INSENSITIVITY_SPAN = 10;
    private long _idCounter = long.MinValue;

    private readonly SemaphoreSlim _collectionLock = new(1);
    private readonly SortedDictionary<long/*tick*/, HashSet<long>/*timerIds*/> _sortedTimeouts = [];
    private readonly Dictionary<long/*timerId*/, TimerInfo> _timeouts = [];

    private readonly TimeProvider _timeProvider;
    private readonly ITimeoutServiceDiagnostic _diagnostic;
    private readonly ITimer _timer;

    private readonly record struct TimerInfo(long Tick, ITimeoutOwner Owner);

    private static async void TimerCallbackAsync(object state)
    {
        var timeoutService = (TimeoutService)state!;
        timeoutService._diagnostic.TimerCallbackCall();

        KeyValuePair<long, HashSet<long>>[] readyTimeoutes;

        try
        {
            await timeoutService._collectionLock.WaitAsync();
            var now1 = timeoutService._timeProvider.GetTimestamp();

            try
            {
                readyTimeoutes = [.. timeoutService._sortedTimeouts.TakeWhile(x => x.Key < now1 + INSENSITIVITY_SPAN)];
                foreach (var smallestTimeout in readyTimeoutes)
                {
                    timeoutService._sortedTimeouts.Remove(smallestTimeout.Key);
                }
            }
            finally
            {
                timeoutService._diagnostic.TimerCallbackDuration(timeoutService._timeProvider.GetElapsedTime(now1).TotalNanoseconds);
                timeoutService._collectionLock.Release();
            }

            foreach (var readyTimeout in readyTimeoutes)
            {
                foreach (var rt in readyTimeout.Value)
                {
                    if (timeoutService._timeouts.TryGetValue(rt, out var timerInfo))
                    {
                        timerInfo.Owner.NotifyTimeoutReady(rt);
                    }
                }
            }

            var now2 = timeoutService._timeProvider.GetTimestamp();

            var delay = Timeout.InfiniteTimeSpan;
            if (timeoutService._sortedTimeouts.Count != 0)
            {
                delay = TimeSpan.FromMilliseconds(timeoutService._sortedTimeouts.First().Key - now2).AlignToValidValue();
            }

            timeoutService._timer.Change(delay, Timeout.InfiniteTimeSpan);
        }
        catch (Exception ex)
        {
            timeoutService._logger.LogCritical(ex, "TimeoutService Callback Error");
        }
    }

    public TimeoutService(TimeProvider timeProvider, ITimeoutServiceDiagnostic diagnostic, ILogger<TimeoutService> logger)
    {
        _timeProvider = timeProvider;
        _diagnostic = diagnostic;
        _logger = logger;
        _timer = _timeProvider.CreateTimer(TimerCallbackAsync!, this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    async Task<long> ITimeoutService.CreateTimeoutAsync(ITimeoutOwner owner, TimeSpan timeout, CancellationToken cancellationToken)
    {
        _diagnostic.CreateTimeoutCall();

        var result = Interlocked.Increment(ref _idCounter);

        await _collectionLock.WaitAsync(cancellationToken);

        var now = _timeProvider.GetTimestamp();
        var ticksToFire = now + (long)timeout.TotalMilliseconds;

        try
        {
            // создаем новый тик
            if (!_sortedTimeouts.TryGetValue(ticksToFire, out var timeoutIds) || timeoutIds == null)
            {
                _sortedTimeouts[ticksToFire] = timeoutIds = [];
            }

            // добавляем идентификатор таймера на момент срабатывания ticksToFire
            timeoutIds.Add(result);
            _timeouts[result] = new TimerInfo() { Tick = ticksToFire, Owner = owner };

            // расчитываем время пробуждения
            var delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
            _timer.Change(delay, Timeout.InfiniteTimeSpan);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CreateTimeoutAsync");
            throw;
        }
        finally
        {
            _diagnostic.CreateTimeoutDuration(_timeProvider.GetElapsedTime(now).TotalNanoseconds);
            _collectionLock.Release();
        }

        return result;
    }

    async Task ITimeoutService.RestartTimeoutAsync(ITimeoutOwner owner, long timeoutId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        _diagnostic.RestartTimeoutCall();

        var changeDetected = false;

        await _collectionLock.WaitAsync(cancellationToken);

        var now = _timeProvider.GetTimestamp();
        var ticksToFire = now + (long)timeout.TotalMilliseconds * TimeSpan.TicksPerMillisecond;

        try
        {
            if (_timeouts.TryGetValue(timeoutId, out var timerInfo))
            {
                if (_sortedTimeouts.TryGetValue(timerInfo.Tick, out var timeoutIds) && timeoutIds != null)
                {
                    if (timeoutIds.Remove(timeoutId) && timeoutIds.Count == 0)
                    {
                        _sortedTimeouts.Remove(timerInfo.Tick);
                    }
                }

                _timeouts[timeoutId] = new TimerInfo(ticksToFire, owner);

                // создаем новый тик
                if (!_sortedTimeouts.TryGetValue(ticksToFire, out var timeoutIds1) || timeoutIds1 == null)
                {
                    _sortedTimeouts[ticksToFire] = timeoutIds1 = [];
                }

                // добавляем идентификатор таймера на момент срабатывания ticksToFire
                timeoutIds1.Add(timeoutId);
                changeDetected = true;
            }

            if (changeDetected)
            {
                var delay = Timeout.InfiniteTimeSpan;
                if (_sortedTimeouts.Count != 0)
                {
                    delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
                }

                _timer.Change(delay, Timeout.InfiniteTimeSpan);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RestartTimeoutAsync");
            throw;
        }
        finally
        {
            _diagnostic.RestartTimeoutDuration(_timeProvider.GetElapsedTime(now).TotalNanoseconds);
            _collectionLock.Release();
        }
    }

    async Task ITimeoutService.CancelTimeoutAsync(ITimeoutOwner owner, long timeoutId, CancellationToken cancellationToken)
    {
        _diagnostic.CancelTimeoutCall();
        var removeDetected = false;

        await _collectionLock.WaitAsync(cancellationToken);

        var now = _timeProvider.GetTimestamp();

        try
        {
            if (_timeouts.TryGetValue(timeoutId, out var timerInfo))
            {
                _timeouts.Remove(timeoutId);
                if (_sortedTimeouts.TryGetValue(timerInfo.Tick, out var timeoutIds) && timeoutIds != null)
                {
                    if (timeoutIds.Remove(timeoutId) && timeoutIds.Count == 0)
                    {
                        _sortedTimeouts.Remove(timerInfo.Tick);
                        removeDetected = true;
                    }
                }
            }

            if (removeDetected)
            {
                var delay = Timeout.InfiniteTimeSpan;
                if (_sortedTimeouts.Count != 0)
                {
                    delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
                }

                _timer.Change(delay, Timeout.InfiniteTimeSpan);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CancelTimeoutAsync");
            throw;
        }
        finally
        {
            _diagnostic.CancelTimeoutDuration(_timeProvider.GetElapsedTime(now).TotalNanoseconds);
            _collectionLock.Release();
        }
    }

    void IDisposable.Dispose()
    {
        _timer.Dispose();
        _collectionLock.Dispose();
    }
}
