using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public abstract class Subscriber<T, TContext> : SubscriberBase<T, TContext>
{
    protected abstract Channel<T> Channel { get; }

    private readonly Func<T, TContext, CancellationToken, Task> _onNext;

    protected void Initialize()
    {
        Subscribe = DataSource.Where(x => Filter(x, Context)).Subscribe(
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
                                await _onNext(value, Context, token);
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

    public Subscriber(IDataSource<T> dataSource, TContext context,
        Func<T, TContext, CancellationToken, Task> onNext,
        Action<Exception>? onError,
        Action? onComplite,
        Func<T, TContext, bool>? filter) : base(dataSource, context, onError, onComplite, filter)
    {
        _onNext = onNext;
    }
}