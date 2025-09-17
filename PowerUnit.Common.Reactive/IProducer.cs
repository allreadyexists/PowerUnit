namespace PowerUnit.Common.Reactive;

/// <summary>
/// Interface with variance annotation; allows for better type checking when detecting capabilities in SubscribeSafe.
/// </summary>
/// <typeparam name="TSource">Type of the resulting sequence's elements.</typeparam>
internal interface IProducer<out TSource> : IObservable<TSource>
{
    IDisposable SubscribeRaw(IObserver<TSource> observer, bool enableSafeguard);
}
