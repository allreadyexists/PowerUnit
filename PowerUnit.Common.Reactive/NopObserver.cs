namespace PowerUnit.Common.Reactive;

internal sealed class NopObserver<T> : IObserver<T>
{
    public static readonly IObserver<T> Instance = new NopObserver<T>();

    public void OnCompleted() { }

    public void OnError(Exception error) { }

    public void OnNext(T value) { }
}
