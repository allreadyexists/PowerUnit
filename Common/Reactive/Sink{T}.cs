using System.Reactive.Disposables;

namespace PowerUnit.Common.Reactive;

internal abstract class Sink<TTarget> : ISink<TTarget>, IDisposable
{
    private SingleAssignmentDisposableValue _upstream;
    private volatile IObserver<TTarget> _observer;

    protected Sink(IObserver<TTarget> observer)
    {
        _observer = observer;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _observer, NopObserver<TTarget>.Instance) != NopObserver<TTarget>.Instance)
            Dispose(true);
    }

    /// <summary>
    /// Override this method to dispose additional resources.
    /// The method is guaranteed to be called at most once.
    /// </summary>
    /// <param name="disposing">If true, the method was called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        //Calling base.Dispose(true) is not a proper disposal, so we can omit the assignment here.
        //Sink is internal so this can pretty much be enforced.
        //_observer = NopObserver<TTarget>.Instance;

        _upstream.Dispose();
    }

    public void ForwardOnNext(TTarget value)
    {
        _observer.OnNext(value);
    }

    public void ForwardOnCompleted()
    {
        _observer.OnCompleted();
        Dispose();
    }

    public void ForwardOnError(Exception error)
    {
        _observer.OnError(error);
        Dispose();
    }

    protected void SetUpstream(IDisposable upstream)
    {
        _upstream.Disposable = upstream;
    }

    protected void DisposeUpstream()
    {
        _upstream.Dispose();
    }
}
