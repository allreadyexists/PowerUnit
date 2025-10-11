using PowerUnit.Common.Shared;
using PowerUnit.Common.StringHelpers;

using System.Buffers;
using System.Collections.Immutable;

internal sealed class Program
{
    private static async ValueTask SomeAction(Shared<IReadOnlyList<byte>> value, CancellationToken ct)
    {
        await Task.Delay((int)(Random.Shared.NextDouble() * 1000), ct);
        Console.WriteLine(string.Format("[{0}] {1}", TimeProvider.System.GetUtcNow().ToString(), string.Join('_', value.Value.Select(x => x.ToHex()))));
        value.Dispose();
    }

    private static async Task Main()
    {
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (var shared = new Shared<IReadOnlyList<byte>>(
            static (ctx) =>
            {
                Console.WriteLine(string.Format("[{0}] Create", TimeProvider.System.GetUtcNow().ToString()));
                var array = ArrayPool<byte>.Shared.Rent(16);
                Random.Shared.NextBytes(array);
                return array;
            },
            static (obj) =>
            {
                ArrayPool<byte>.Shared.Return((byte[])obj);
                Console.WriteLine(string.Format("[{0}] Dispose", TimeProvider.System.GetUtcNow().ToString()));
            }))
        {
            var sharedRefs = Enumerable.Range(1, 16).Select(x => shared.AddRef()).ToImmutableArray();
            await Parallel.ForEachAsync(sharedRefs, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1),
            }, body: SomeAction);
        }
#pragma warning restore IDE0063 // Use simple 'using' statement
    }
}