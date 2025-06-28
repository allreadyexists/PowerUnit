using System.Collections;

namespace PowerUnit.Common.FileExtension;

public sealed class FileContent : IEnumerable<List<byte>>
{
    private readonly string _path;
    private readonly int _sectionMaxLength;

    public FileContent(string path, int sectionMaxLength)
    {
        _path = path;
        _sectionMaxLength = sectionMaxLength;
    }

    IEnumerator<List<byte>> IEnumerable<List<byte>>.GetEnumerator() => new FileContentEnumerator(_path, _sectionMaxLength);
    IEnumerator IEnumerable.GetEnumerator() => new FileContentEnumerator(_path, _sectionMaxLength);
}
