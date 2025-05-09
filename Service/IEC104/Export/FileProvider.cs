using System.Collections.Concurrent;

namespace PowerUnit;

internal static class FileInfoExtension
{
    public static bool IsReady(this FileInfo? fileInfo)
    {
        return fileInfo?.Length <= 0xFFFFFF;
    }
}

internal sealed class FileProvider : IFileProvider, IDisposable
{
    private readonly string _cachePath;

    private readonly ConcurrentDictionary<ushort, FileStream> _fileStreams = new ConcurrentDictionary<ushort, FileStream>();

    public FileProvider(IEnviromentManager enviromentManager)
    {
        _cachePath = enviromentManager.GetCachePath();
    }

    void IDisposable.Dispose()
    {
        foreach (var fileReadState in _fileStreams)
        {
            fileReadState.Value.Close();
            fileReadState.Value.Dispose();
        }

        _fileStreams.Clear();
    }

    IEnumerable<FileSystemItem> IFileProvider.GetDirectoryContent(uint address, ushort fileName)
    {
        foreach (var item in Directory.EnumerateFiles(_cachePath))
        {
            var fileNameWithoutExtension = Path.GetFileName(item);
            if (ushort.TryParse(fileNameWithoutExtension, out var fileNameAsNumber))
            {
                var fileInfo = new FileInfo(item);
                if (fileInfo.Length < 0xFFFFFF)
                {
                    yield return new FileSystemItem(fileNameAsNumber, (uint)fileInfo.Length, 0, fileInfo.CreationTimeUtc);
                }
            }
        }
    }
    private FileStream CreateFileReadState(ushort fileName)
    {
        var file = Path.Combine(_cachePath, $"{fileName}");
        return File.OpenRead(file);
    }

    private static uint SectionLength(int fileSize, byte sectionName, uint sectionMaxLength)
    {
        var sectionOffset = (sectionName - 1) * sectionMaxLength;
        if (sectionOffset + sectionMaxLength < fileSize)
            return sectionMaxLength;
        else
        {
            if (fileSize > sectionOffset)
                return (uint)(fileSize - sectionOffset);
            else
                return 0;
        }
    }

    byte[] IFileProvider.GetSection(ushort fileName, byte sectionName, uint sectionMaxLength)
    {
        var fileStream = _fileStreams.AddOrUpdate(fileName, CreateFileReadState, (fileName, oldValue) => oldValue);

        var sectionLength = SectionLength((int)fileStream.Length, sectionName, sectionMaxLength);
        var buffer = new byte[sectionLength];
        var offset = (int)((sectionName - 1) * sectionMaxLength);

        fileStream.Seek(offset, SeekOrigin.Begin);
        _ = fileStream.Read(buffer, 0, (int)sectionLength);

        return buffer;
    }

    FileInfo? IFileProvider.GetFileInfo(ushort fileName)
    {
        var file = Path.Combine(_cachePath, $"{fileName}");
        return File.Exists(file) ? new FileInfo(file) : null;
    }

    void IFileProvider.CloseFile(ushort fileName)
    {
        if (_fileStreams.TryRemove(fileName, out var fileStream))
        {
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}

