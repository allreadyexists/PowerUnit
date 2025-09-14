using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class SubscriberBounded<T, TContext> : Subscriber<T, TContext>
{
    private readonly Channel<T> _channel;
    protected override Channel<T> Channel => _channel;

    public SubscriberBounded(int capacity, IDataSource<T> dataSource, TContext context, Func<T, TContext, CancellationToken, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null, Func<T, TContext, bool>? filter = null, ISubscriberDiagnostic? subscriberDiagnostic = null) : base(dataSource, context, onNext, onError, onComplite, filter, subscriberDiagnostic)
    {
        _channel = System.Threading.Channels.Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        }, dropped =>
        {
            SubscriberDiagnostic?.DropCounter(typeof(TContext).Name, typeof(T).Name);
        });

        Initialize();
    }
}
