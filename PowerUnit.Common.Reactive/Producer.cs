using System.Reactive.Concurrency;

namespace PowerUnit.Common.Reactive;

internal abstract class Producer<TTarget, TSink> : IProducer<TTarget>
        where TSink : IDisposable
{
    /// <summary>
    /// Publicly visible Subscribe method.
    /// </summary>
    /// <param name="observer">Observer to send notifications on. The implementation of a producer must ensure the correct message grammar on the observer.</param>
    /// <returns>IDisposable to cancel the subscription. This causes the underlying sink to be notified of unsubscription, causing it to prevent further messages from being sent to the observer.</returns>
    public IDisposable Subscribe(IObserver<TTarget> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        return SubscribeRaw(observer, enableSafeguard: true);
    }

    public IDisposable SubscribeRaw(IObserver<TTarget> observer, bool enableSafeguard)
    {
        ISafeObserver<TTarget>? safeObserver = null;

        //
        // See AutoDetachObserver.cs for more information on the safeguarding requirement and
        // its implementation aspects.
        //
        if (enableSafeguard)
        {
            observer = safeObserver = SafeObserver<TTarget>.Wrap(observer);
        }

        var sink = CreateSink(observer);

        safeObserver?.SetResource(sink);

        if (CurrentThreadScheduler.IsScheduleRequired)
        {
            CurrentThreadScheduler.Instance.ScheduleAction(
                (@this: this, sink),
                static tuple => tuple.@this.Run(tuple.sink));
        }
        else
        {
            Run(sink);
        }

        return sink;
    }

    /// <summary>
    /// Core implementation of the query operator, called upon a new subscription to the producer object.
    /// </summary>
    /// <param name="sink">The sink object.</param>
    protected abstract void Run(TSink sink);

    protected abstract TSink CreateSink(IObserver<TTarget> observer);
}
