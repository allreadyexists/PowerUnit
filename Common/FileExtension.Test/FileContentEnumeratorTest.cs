using System.Runtime.InteropServices;

namespace PowerUnit.Common.FileExtension;

public class FileContentEnumeratorTest
{
    private string _file;

    private static async Task FillFile(Stream stream, int blockCount, int blockSize)
    {
        var block = new byte[blockSize];

        for (var i = 0; i < blockCount; i++)
        {
            Random.Shared.NextBytes(block);
            await stream.WriteAsync(block);
        }

        await stream.FlushAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _file = Path.GetRandomFileName();
        using var _stream = File.Create(_file);
        await FillFile(_stream, 13, 17);
    }

    [Test]
    public void FileContentByBlocks_Enumerator()
    {
        using var enumerator = new FileContentEnumerator(_file, 23);
        while (enumerator.MoveNext())
        {
            var block = enumerator.Current;
            Console.WriteLine(CollectionsMarshal.AsSpan(block).ToHex());
        }
    }

    [Test]
    public void FileContentByBlocks_Enumerable()
    {
        foreach (var block in new FileContent(_file, 31))
        {
            Console.WriteLine(CollectionsMarshal.AsSpan(block).ToHex());
        }
    }

    [TearDown]
    public void TearDown()
    {
        File.Delete(_file);
    }
}
