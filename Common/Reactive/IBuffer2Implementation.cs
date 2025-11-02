namespace PowerUnit.Common.Reactive;

internal interface IBuffer2Implementation
{
    IObservable<List<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count);
}
