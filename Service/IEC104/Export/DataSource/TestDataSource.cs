using Microsoft.Extensions.Logging;

using PowerUnit.Common.Subsciption;

namespace PowerUnit.Service.IEC104.Export.DataSource;

internal abstract class TestDataSource<T> : DataSourceBase<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TestDataSource<T>> _logger;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;

    private static T Callback(DataSourceBase<T> ctx)
    {
        var curCtx = (TestDataSource<T>)ctx;
        return curCtx.CreateNewValue(curCtx._timeProvider.GetUtcNow().DateTime);
    }

    public TestDataSource(TimeProvider timeProvider, ILogger<TestDataSource<T>> logger) : base(logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        new Task(static /*async */(value) =>
        {
            var source = (TestDataSource<T>)value!;
            while (!source._cancellationToken.IsCancellationRequested)
            {
                try
                {
                    source.Notify(Callback);
                    Thread.SpinWait(1);
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
                    source._logger.LogError(ex, string.Empty);
                }
            }
        }, this, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning).Start();
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    protected abstract T CreateNewValue(DateTime now);
}
