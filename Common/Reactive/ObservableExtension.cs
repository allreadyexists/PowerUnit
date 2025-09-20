namespace PowerUnit.Common.Reactive;

public static class ObservableExtension
{
    private static readonly Buffer2Implementation _impl = new Buffer2Implementation();
    public static IObservable<IList<TSource>> Buffer2<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(timeSpan, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        return _impl.Buffer2(source, timeSpan, count);
    }
}
