using PowerUnit.Common.Reactive;

using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class BatchSubscriber<T, TContext> : SubscriberBase<T, TContext>
{
    private readonly Channel<IList<T>> _channel;

    public BatchSubscriber(int count, TimeSpan timeSpan, IDataSource<T> dataSource, TContext context,
        Func<IList<T>, TContext, CancellationToken, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null,
        Func<T, TContext, bool>? filter = null, ISubscriberDiagnostic? subscriberDiagnostic = null) : base(dataSource, context, onError, onComplite, filter, subscriberDiagnostic)
    {
        _channel = Channel.CreateUnbounded<IList<T>>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        Subscribe = DataSource.Where(x => Filter(x, Context)).Buffer2(timeSpan, count).Subscribe(
            value =>
            {
                _channel.Writer.TryWrite(value);
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
                    while (await _channel.Reader.WaitToReadAsync(token))
                    {
                        if (_channel.Reader.TryRead(out var value))
                        {
                            try
                            {
                                await onNext(value, Context, token);
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
            catch (Exception ocex)
            {
                OnError(ocex);
            }
            finally
            {
                OnComplite();
            }
        }
        , TokenSource.Token);
    }
}