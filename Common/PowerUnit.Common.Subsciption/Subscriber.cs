using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public abstract class Subscriber<T> : SubscriberBase<T>
{
    protected abstract Channel<T> Channel { get; }

    private readonly Func<T, Task> _onNext;

    protected void Initialize()
    {
        Subscribe = DataSource.Where(x => Filter(x)).Subscribe(
            value => Channel.Writer.TryWrite(value),
            OnError,
            OnComplite);
        _ = Task.Run(async () =>
        {
            var token = TokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Channel != null)
                    {
                        await foreach (var value in Channel.Reader.ReadAllAsync(token))
                        {
                            try
                            {
                                await _onNext(value);
                            }
                            catch (Exception ex)
                            {
                                OnError(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ocex)
            {
                OnError(ocex);
            }
            finally
            {
                OnComplite();
            }
        }, TokenSource.Token);
    }

    public Subscriber(IDataSource<T> dataSource,
        Func<T, Task> onNext,
        Action<Exception>? onError,
        Action? onComplite,
        Predicate<T>? filter) : base(dataSource, onError, onComplite, filter)
    {
        _onNext = onNext;
    }
}