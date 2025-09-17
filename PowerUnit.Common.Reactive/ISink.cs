namespace PowerUnit.Common.Reactive;

internal interface ISink<in TTarget>
{
    void ForwardOnNext(TTarget value);
    void ForwardOnCompleted();
    void ForwardOnError(Exception error);
}
