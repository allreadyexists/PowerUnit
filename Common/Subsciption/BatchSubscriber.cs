using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class BatchSubscriber<T, TContext> : SubscriberBase<T, TContext>
{
    private readonly Channel<IEnumerable<T>> _channel;

    public BatchSubscriber(int count, TimeSpan timeSpan, IDataSource<T> dataSource, TContext context,
        Func<IEnumerable<T>, TContext, CancellationToken, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null,
        Func<T, TContext, bool>? filter = null) : base(dataSource, context, onError, onComplite, filter)
    {
        _channel = Channel.CreateUnbounded<IEnumerable<T>>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        Subscribe = DataSource.Where(x => Filter(x, Context)).Buffer(timeSpan, count).Subscribe(
            value => _channel.Writer.TryWrite(value),
            OnError,
            OnComplite);
        _ = Task.Run(async () =>
        {
            var token = TokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await foreach (var value in _channel.Reader.ReadAllAsync(token))
                    {
                        try
                        {
                            await onNext(value, Context, token);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
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