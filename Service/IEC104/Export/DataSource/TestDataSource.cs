using PowerUnit.Common.Subsciption;

namespace PowerUnit.Service.IEC104.Export.DataSource;

internal abstract class TestDataSource<T> : DataSourceBase<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TestDataSource<T>> _logger;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public TestDataSource(TimeProvider timeProvider, ILogger<TestDataSource<T>> logger) : base(logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var now = _timeProvider.GetUtcNow().DateTime;
                try
                {
                    Notify(CreateNewValue(now));
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Empty);
                }
            }
        }, _cancellationTokenSource.Token);
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    protected abstract T CreateNewValue(DateTime now);
}
