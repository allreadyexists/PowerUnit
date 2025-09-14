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
            value =>
            {
                Channel.Writer.TryWrite(value);
                SubscriberDiagnostic?.RcvCounter(typeof(TContext).Name, typeof(T).Name);
            },
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
                        while (await Channel.Reader.WaitToReadAsync(token))
                        {
                            if (Channel.Reader.TryRead(out var value))
                            {
                                try
                                {
                                    await _onNext(value, Context, token);
                                    SubscriberDiagnostic?.ProcessCounter(typeof(TContext).Name, typeof(T).Name);
                                }
                                catch (Exception ex)
                                {
                                    OnError(ex);
                                }
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
        Func<T, TContext, bool>? filter, ISubscriberDiagnostic? subscriberDiagnostic) : base(dataSource, context, onError, onComplite, filter, subscriberDiagnostic)
    {
        _onNext = onNext;
    }
}