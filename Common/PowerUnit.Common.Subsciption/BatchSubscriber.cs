using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class BatchSubscriber<T> : IDisposable
{
    private readonly IDisposable _subscribe;
    private readonly IDataSource<T> _dataSource;
    private readonly CancellationTokenSource _tokenSource;

    private readonly Action<Exception> _onError;
    private readonly Action _onComplite;

    private readonly Channel<IEnumerable<T>> _channel;

    public BatchSubscriber(int count, TimeSpan timeSpan, IDataSource<T> dataSource,
        Func<IEnumerable<T>, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null,
        Predicate<T>? filter = null)
    {
        _dataSource = dataSource;
        _tokenSource = new CancellationTokenSource();

        _channel = Channel.CreateUnbounded<IEnumerable<T>>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        _onError = onError ?? DelegateHelper.Empty<Exception>();
        _onComplite = onComplite ?? DelegateHelper.Empty();

        var valueFilter = filter ?? (static (value) => true);

        _subscribe = _dataSource.Where(x => valueFilter(x)).Buffer(timeSpan, count).Subscribe(
            value => _channel.Writer.TryWrite(value),
            _onError,
            _onComplite);
        _ = Task.Run(async () =>
        {
            var token = _tokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await foreach (var value in _channel.Reader.ReadAllAsync(token))
                    {
                        try
                        {
                            await onNext(value);
                        }
                        catch (Exception ex)
                        {
                            _onError(ex);
                        }
                    }
                }
            }
            catch (Exception ocex)
            {
                _onError(ocex);
            }
            finally
            {
                _onComplite();
            }
        }
        , _tokenSource.Token);
    }

    void IDisposable.Dispose()
    {
        _subscribe.Dispose();
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}