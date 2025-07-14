using Microsoft.Extensions.Logging;

using Timer = System.Threading.Timer;

namespace PowerUnit;

internal sealed class TimeoutService : ITimeoutService, IDisposable
{
    private readonly ILogger<TimeoutService> _logger;

    private const int INSENSITIVITY_SPAN = 10;
    private long _idCounter = long.MinValue;

    private readonly SemaphoreSlim _tickLock = new(1);
    private int _lastCallTicks;
    private int _overflowsCounter;

    private readonly SemaphoreSlim _collectionLock = new(1);
    private readonly SortedDictionary<long/*tick*/, HashSet<long>/*timerIds*/> _sortedTimeouts = [];
    private readonly Dictionary<long/*timerId*/, TimerInfo> _timeouts = [];

    private readonly Timer _timer;

    private sealed class TimerInfo
    {
        public long Tick { get; set; }
        public required ITimeoutOwner Owner { get; set; }
    }

    private async void TimerCallbackAsync(object state)
    {
        var currentTicks = await GetTicksLongAsync(default);
        KeyValuePair<long, HashSet<long>>[] readyTimeoutes;

        try
        {
            await _collectionLock.WaitAsync();
            readyTimeoutes = [.. _sortedTimeouts.TakeWhile(x => x.Key < currentTicks + INSENSITIVITY_SPAN)];
            foreach (var smallestTimeout in readyTimeoutes)
            {
                _sortedTimeouts.Remove(smallestTimeout.Key);
            }
        }
        finally
        {
            _collectionLock.Release();
        }

        foreach (var readyTimeout in readyTimeoutes)
        {
            foreach (var rt in readyTimeout.Value)
            {
                if (_timeouts.TryGetValue(rt, out var timerInfo))
                {
                    _ = timerInfo.Owner.NotifyTimeoutReadyAsync(rt, default);
                }
            }
        }

        var now = await GetTicksLongAsync(default);

        var delay = TimeSpanHelper.WaitMaxValue;
        if (_sortedTimeouts.Count != 0)
        {
            delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
        }

        _timer.Change(delay, TimeSpanHelper.WaitMaxValue);
    }

    public TimeoutService(ILogger<TimeoutService> logger)
    {
        _logger = logger;
        _timer = new Timer(TimerCallbackAsync!, this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async Task<long> GetTicksLongAsync(CancellationToken cancellationToken)
    {
        await _tickLock.WaitAsync(cancellationToken);

        try
        {
            var currentTicks = Environment.TickCount;
            if (_lastCallTicks > currentTicks)
                _overflowsCounter++;
            _lastCallTicks = currentTicks;
            return (long)_overflowsCounter * int.MaxValue + currentTicks;
        }
        finally
        {
            _tickLock.Release();
        }
    }

    async Task<long> ITimeoutService.CreateTimeoutAsync(ITimeoutOwner owner, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var result = Interlocked.Increment(ref _idCounter);
        try
        {
            var now = await GetTicksLongAsync(cancellationToken);
            var ticksToFire = now + (long)timeout.TotalMilliseconds;

            await _collectionLock.WaitAsync(cancellationToken);

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
                _timer.Change(delay, TimeSpanHelper.WaitMaxValue);
            }
            finally
            {
                _collectionLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CreateTimeoutAsync");
        }

        return result;
    }

    async Task ITimeoutService.RestartTimeoutAsync(ITimeoutOwner owner, long timeoutId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var changeDetected = false;

        await _collectionLock.WaitAsync(cancellationToken);

        try
        {
            var now = await GetTicksLongAsync(cancellationToken);
            var ticksToFire = now + (long)timeout.TotalMilliseconds;

            if (_timeouts.TryGetValue(timeoutId, out var timerInfo))
            {
                if (_sortedTimeouts.TryGetValue(timerInfo.Tick, out var timeoutIds) && timeoutIds != null)
                {
                    if (timeoutIds.Remove(timeoutId) && timeoutIds.Count == 0)
                    {
                        _sortedTimeouts.Remove(timerInfo.Tick);
                    }
                }

                timerInfo.Tick = ticksToFire;
                timerInfo.Owner = owner;

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
                var delay = TimeSpanHelper.WaitMaxValue;
                if (_sortedTimeouts.Count != 0)
                {
                    delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
                }

                _timer.Change(delay, TimeSpanHelper.WaitMaxValue);
            }
        }
        finally
        {
            _collectionLock.Release();
        }
    }

    async Task ITimeoutService.CancelTimeoutAsync(ITimeoutOwner owner, long timeoutId, CancellationToken cancellationToken)
    {
        var removeDetected = false;

        await _collectionLock.WaitAsync(cancellationToken);

        try
        {
            var now = await GetTicksLongAsync(cancellationToken);

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
                var delay = TimeSpanHelper.WaitMaxValue;
                if (_sortedTimeouts.Count != 0)
                {
                    delay = TimeSpan.FromMilliseconds(_sortedTimeouts.First().Key - now).AlignToValidValue();
                }

                _timer.Change(delay, TimeSpanHelper.WaitMaxValue);
            }
        }
        finally
        {
            _collectionLock.Release();
        }
    }

    void IDisposable.Dispose()
    {
        _timer.Dispose();
        _tickLock.Dispose();
        _collectionLock.Dispose();
    }
}
