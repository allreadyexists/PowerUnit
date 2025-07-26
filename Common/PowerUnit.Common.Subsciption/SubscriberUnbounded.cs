using System.Threading.Channels;

namespace PowerUnit.Common.Subsciption;

public sealed class SubscriberUnbounded<T> : Subscriber<T>
{
    private readonly Channel<T> _channel;
    protected override Channel<T> Channel => _channel;

    public SubscriberUnbounded(IDataSource<T> dataSource, Func<T, Task> onNext,
        Action<Exception>? onError = null,
        Action? onComplite = null, Predicate<T>? filter = null) : base(dataSource, onNext, onError, onComplite, filter)
    {
        _channel = System.Threading.Channels.Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        Initialize();
    }
}