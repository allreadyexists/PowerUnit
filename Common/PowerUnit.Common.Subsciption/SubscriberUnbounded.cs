using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class SubscriberUnbounded<T, TContext> : Subscriber<T, TContext>
{
    private readonly Channel<T> _channel;
    protected override Channel<T> Channel => _channel;

    public SubscriberUnbounded(IDataSource<T> dataSource, TContext context, Func<T, TContext, CancellationToken, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null, Func<T, TContext, bool>? filter = null) : base(dataSource, context, onNext, onError, onComplite, filter)
    {
        _channel = System.Threading.Channels.Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        Initialize();
    }
}