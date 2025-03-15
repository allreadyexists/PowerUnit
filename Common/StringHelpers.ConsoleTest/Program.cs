namespace K2.Common.StringHelpers.ConsoleTest;

internal sealed class Program
{
    internal static void Main()
    {
        var array = new byte[] { 1, 2, 3, 4, 10, 11, 12, 13, 233, 234, 235, 236 };
        Console.WriteLine(array.ToHex());
    }
}
