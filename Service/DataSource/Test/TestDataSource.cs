using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PowerUnit.Common.Subsciption;

namespace PowerUnit.DataSource.Test;

internal abstract class TestDataSource<T> : DataSourceBase<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITestDataSourceDiagnostic _testDataSourceDiagnostic;
    private readonly ILogger<TestDataSource<T>> _logger;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;

    private readonly uint _intervalUs;
    private long _diffUs;
    private readonly PeriodicTimer _timer;

    private static T Callback(DataSourceBase<T> ctx)
    {
        var curCtx = (TestDataSource<T>)ctx;
        return curCtx.CreateNewValue(curCtx._timeProvider.GetUtcNow().DateTime);
    }

    public TestDataSource(IOptions<BaseValueTestDataSourceOptions> options, TimeProvider timeProvider, ITestDataSourceDiagnostic testDataSourceDiagnostic, ILogger<TestDataSource<T>> logger) : base(logger)
    {
        _timeProvider = timeProvider;
        _testDataSourceDiagnostic = testDataSourceDiagnostic;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _intervalUs = 1_000_000 / options.Value.Rps;
        _timer = new PeriodicTimer(TimeSpan.FromMicroseconds(_intervalUs < 1000 ? 1000 : _intervalUs));

        new Task(static async (value) =>
        {
            var source = (TestDataSource<T>)value!;
            var ct = source._cancellationToken;
            var sw = source._timeProvider.GetTimestamp();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var elapsed = (long)source._timeProvider.GetElapsedTime(sw).TotalMicroseconds + source._diffUs;
                    var batchSize = elapsed / source._intervalUs;
                    source._diffUs = elapsed % source._intervalUs;
                    sw = source._timeProvider.GetTimestamp();

                    for (var i = 0; i < batchSize; i++)
                    {
                        if (ct.IsCancellationRequested)
                            return;
                        source.Notify(Callback);
                        source._testDataSourceDiagnostic.IncRequest();
                    }

                    await source._timer.WaitForNextTickAsync(ct);
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
                    await Task.Delay(1_000, ct);
                }
            }
        }, this, _cancellationTokenSource.Token).Start();
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    protected abstract T CreateNewValue(DateTime now);
}
