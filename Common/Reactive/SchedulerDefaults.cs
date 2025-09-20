using System.Reactive.Concurrency;

namespace PowerUnit.Common.Reactive;

internal static class SchedulerDefaults
{
    internal static IScheduler ConstantTimeOperations => ImmediateScheduler.Instance;
    internal static IScheduler TailRecursion => ImmediateScheduler.Instance;
    internal static IScheduler Iteration => CurrentThreadScheduler.Instance;
    internal static IScheduler TimeBasedOperations => DefaultScheduler.Instance;
    internal static IScheduler AsyncConversions => DefaultScheduler.Instance;
}
