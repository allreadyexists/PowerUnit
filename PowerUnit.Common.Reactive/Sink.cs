namespace PowerUnit.Common.Reactive;

/// <summary>
/// Base class for implementation of query operators, providing a lightweight sink that can be disposed to mute the outgoing observer.
/// </summary>
/// <typeparam name="TTarget">Type of the resulting sequence's elements.</typeparam>
/// <typeparam name="TSource"></typeparam>
/// <remarks>Implementations of sinks are responsible to enforce the message grammar on the associated observer. Upon sending a terminal message, a pairing Dispose call should be made to trigger cancellation of related resources and to mute the outgoing observer.</remarks>
internal abstract class Sink<TSource, TTarget> : Sink<TTarget>, IObserver<TSource>
{
    protected Sink(IObserver<TTarget> observer) : base(observer)
    {
    }

    public virtual void Run(IObservable<TSource> source)
    {
        SetUpstream(source.SubscribeSafe(this));
    }

    public abstract void OnNext(TSource value);

    public virtual void OnError(Exception error) => ForwardOnError(error);

    public virtual void OnCompleted() => ForwardOnCompleted();

    public IObserver<TTarget> GetForwarder() => new _(this);

    private sealed class _ : IObserver<TTarget>
    {
        private readonly Sink<TSource, TTarget> _forward;

        public _(Sink<TSource, TTarget> forward)
        {
            _forward = forward;
        }

        public void OnNext(TTarget value) => _forward.ForwardOnNext(value);

        public void OnError(Exception error) => _forward.ForwardOnError(error);

        public void OnCompleted() => _forward.ForwardOnCompleted();
    }
}
