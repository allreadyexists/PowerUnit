using System.Reactive.Linq;
using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class BatchSubscriber<T> : SubscriberBase<T>
{
    private readonly Channel<IEnumerable<T>> _channel;

    public BatchSubscriber(int count, TimeSpan timeSpan, IDataSource<T> dataSource,
        Func<IEnumerable<T>, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null,
        Predicate<T>? filter = null) : base(dataSource, onError, onComplite, filter)
    {
        _channel = Channel.CreateUnbounded<IEnumerable<T>>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        Subscribe = DataSource.Where(x => Filter(x)).Buffer(timeSpan, count).Subscribe(
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
                            await onNext(value);
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