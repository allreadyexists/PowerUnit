using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PowerUnit.Common.FileExtension;

public class FileContentEnumerator : IEnumerator<List<byte>>
{
    private readonly FileStream _stream;
    private readonly List<byte> _current;

    public FileContentEnumerator(string path, int sectionMaxLength)
    {
        _stream = File.OpenRead(path);
        _current = new List<byte>(sectionMaxLength);
        for (var i = 0; i < sectionMaxLength; i++)
        {
            _current.Add(0);
        }
    }

    public List<byte> Current => _current;
    object IEnumerator.Current => Current;
    public bool MoveNext()
    {
        var span = CollectionsMarshal.AsSpan(_current);
        var length = _stream.Read(span);
        CollectionsMarshal.SetCount(_current, length);
        return length > 0;
    }
    public void Reset() => _stream.Position = 0;
    void IDisposable.Dispose() => _stream.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPosition(int position) => _stream.Position = position;
    public long Length => _stream.Length;
}
