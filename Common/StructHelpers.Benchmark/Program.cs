using BenchmarkDotNet.Running;

using Test;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<StructTest>();
    }
}

