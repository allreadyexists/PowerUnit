using System.Reactive.Concurrency;

namespace PowerUnit.Common.Reactive;

internal sealed class Buffer2Implementation : IBuffer2Implementation
{
    public IObservable<IList<TSource>> Buffer2<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count)
    {
        return Buffer2_(source, timeSpan, count, SchedulerDefaults.TimeBasedOperations);
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IObservable<IList<TSource>> Buffer2_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        return new Buffer2<TSource>.Ferry2(source, timeSpan, count, scheduler);
    }
}
