using System.Reactive.Concurrency;

namespace PowerUnit.Common.Reactive;

internal interface IBuffer2Implementation
{
    IObservable<IList<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count);
    IObservable<IList<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler);
}
