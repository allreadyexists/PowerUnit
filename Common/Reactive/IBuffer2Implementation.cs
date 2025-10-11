namespace PowerUnit.Common.Reactive;

internal interface IBuffer2Implementation
{
    IObservable<IList<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count);
}
