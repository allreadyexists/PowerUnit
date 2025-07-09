using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public abstract class Subscriber<T> : IDisposable
{
    private readonly IDisposable _subscribe;
    private readonly IDataSource<T> _dataSource;
    private readonly CancellationTokenSource _tokenSource;

    private readonly Action<Exception> _onError;
    private readonly Action _onComplite;

    protected abstract Channel<T> Channel { get; }

    public Subscriber(IDataSource<T> dataSource,
        Func<T, Task> onNext,
        Action<Exception>? onError,
        Action? onComplite,
        Predicate<T>? filter)
    {
        _dataSource = dataSource;
        _tokenSource = new CancellationTokenSource();

        _onError = onError ?? DelegateHelper.Empty<Exception>();
        _onComplite = onComplite ?? DelegateHelper.Empty();

        var valueFilter = filter ?? (static (value) => true);

        _subscribe = _dataSource.Where(x => valueFilter(x)).Subscribe(
            value => Channel.Writer.TryWrite(value),
            _onError,
            _onComplite);
        _ = Task.Run(async () =>
        {
            var token = _tokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await foreach (var value in Channel.Reader.ReadAllAsync(token))
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