using System.Reactive.Concurrency;

namespace PowerUnit.Common.Reactive;

internal sealed class Buffer2Implementation : IBuffer2Implementation
{
    public IObservable<List<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count) =>
        Buffer2_(source, timeSpan, count, SchedulerDefaults.TimeBasedOperations);

    private static Buffer2<TSource>.Ferry2 Buffer2_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler) =>
        new Buffer2<TSource>.Ferry2(source, timeSpan, count, scheduler);
}
