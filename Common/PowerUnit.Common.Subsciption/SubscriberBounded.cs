using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class SubscriberBounded<T> : Subscriber<T>
{
    private readonly Channel<T> _channel;
    protected override Channel<T> Channel => _channel;

    public SubscriberBounded(int capacity, IDataSource<T> dataSource, Func<T, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null, Predicate<T>? filter = null) : base(dataSource, onNext, onError, onComplite, filter)
    {
        _channel = System.Threading.Channels.Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        }, dropped => { });
    }
}
