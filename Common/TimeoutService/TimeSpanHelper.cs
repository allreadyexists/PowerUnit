using System.Runtime.CompilerServices;

namespace PowerUnit.Common.TimeoutService;

internal static class TimeSpanHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan AlignToValidValue(this TimeSpan value) =>
        value == Timeout.InfiniteTimeSpan ?
            value :
            value < TimeSpan.Zero ?
                TimeSpan.Zero :
                value > Timeout.InfiniteTimeSpan ?
                    Timeout.InfiniteTimeSpan :
                    value;
}
