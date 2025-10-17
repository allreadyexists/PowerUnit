using System.Runtime.CompilerServices;

namespace PowerUnit.Common.TimeoutService;

internal static class TimeSpanHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan AlignToValidValue(this TimeSpan value) =>
        value == TimeSpan.MaxValue ?
            value :
            value < TimeSpan.Zero ?
                TimeSpan.Zero :
                value > TimeSpan.MaxValue ?
                    TimeSpan.MaxValue :
                    value;
}
