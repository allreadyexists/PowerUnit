namespace PowerUnit.Common.TimeoutService;

internal static class TimeSpanHelper
{
    public static readonly TimeSpan WaitMaxValue = TimeSpan.FromMilliseconds(int.MaxValue);

    public static TimeSpan AlignToValidValue(this TimeSpan value) =>
        value == Timeout.InfiniteTimeSpan ?
            value :
            value < TimeSpan.Zero ?
                TimeSpan.Zero :
                value > WaitMaxValue ?
                    WaitMaxValue :
                    value;
}
