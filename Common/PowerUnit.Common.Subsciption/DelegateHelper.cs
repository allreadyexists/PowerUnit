namespace PowerUnit.Common.Subsciption;

internal static class DelegateHelper
{
    public static Action Empty() => static () => { };
    public static Action<T> Empty<T>() => static (value) => { };
    public static Action<T1, T2> Empty<T1, T2>() => static (value1, value2) => { };
}
