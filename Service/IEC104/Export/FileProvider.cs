using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Text;

namespace PowerUnit;

internal sealed class FileProvider : IFileProvider, IDisposable
{
    private int? _id1;
    private Guid? _id2;
    private string _cachePath;
    private readonly IEnviromentManager _enviromentManager;
    private readonly ILogger<FileProvider> _logger;
    private string _extension = string.Empty;
    private readonly string _writeTemplate = "{0}.w.{1}";
    private readonly string _readTemplate = "{0}.r.{1}";

    private readonly ConcurrentDictionary<string, bool> _readFiles = [];
    private readonly ConcurrentDictionary<string, bool> _writeFiles = [];

    private readonly ConcurrentDictionary<ushort, FileStream> _fileStreams = new ConcurrentDictionary<ushort, FileStream>();

    public FileProvider(IEnviromentManager enviromentManager, ILogger<FileProvider> logger)
    {
        _enviromentManager = enviromentManager;
        _logger = logger;
    }

    void IDisposable.Dispose()
    {
        var id = "." + _id2.ToString();
        var allFiles = Directory.EnumerateFiles(_cachePath);
        var myFiles = allFiles.Select(x => Path.GetExtension(x));
        foreach (var file in Directory.EnumerateFiles(_cachePath).Where(x => Path.GetExtension(x) == id))
        {
            File.Delete(file);
        }

        foreach (var fileReadState in _fileStreams)
        {
            fileReadState.Value.Close();
            fileReadState.Value.Dispose();
        }

        _fileStreams.Clear();
    }

    void IFileProvider.SetId(int id1, Guid id2)
    {
        if (_id1 != null)
            throw new ArgumentException(nameof(IFileProvider.SetId), nameof(id1));
        if (_id2 != null)
            throw new ArgumentException(nameof(IFileProvider.SetId), nameof(id2));

        _id1 = id1;
        _id2 = id2;
        _cachePath = Path.Combine(_enviromentManager.GetCachePath(), id1.ToString());

        if (!Directory.Exists(_cachePath))
            Directory.CreateDirectory(_cachePath);

        _extension = $".{_id2}";
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

    bool IFileProvider.PrepareReadFile(ushort fileName/*, int sectionLength*/, out FileReadCache? fileCache)
    {
        var cnt = 3;
        fileCache = default;
        while (true)
        {
            try
            {
                var fileSrc = Path.Combine(_cachePath, $"{fileName}");
                if (!File.Exists(fileSrc))
                    return false;

                var fileDst = Path.Combine(_cachePath, string.Format(_readTemplate, fileName, _id2));
                if (_readFiles.ContainsKey(fileDst))
                    return false;

                var fileInfo = new FileInfo(fileSrc);
                if (fileInfo.Length > 0xFFFFFF)
                    return false;

                File.Copy(fileSrc, fileDst);

                fileInfo = new FileInfo(fileDst);
                if (fileInfo.Length > 0xFFFFFF)
                    return false;

                var sectionLength = Math.Floor(fileInfo.Length / 128m);
                if (sectionLength == 0)
                {
                    sectionLength = fileInfo.Length;
                }

                fileCache = new FileReadCache(fileDst, (int)fileInfo.Length, (int)sectionLength);
                _readFiles.TryAdd(fileDst, true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(IFileProvider.PrepareReadFile));
                cnt--;
                if (cnt == 0)
                    return false;
            }
        }
    }

    bool IFileProvider.PrepareWriteFile(ushort fileName/*, int sectionLength*/, out FileWriteCache? fileCache)
    {
        var cnt = 3;
        fileCache = default;
        while (true)
        {
            try
            {
                var fileDst = Path.Combine(_cachePath, string.Format(_writeTemplate, fileName, _id2));
                if (_writeFiles.ContainsKey(fileDst))
                    return false;

                using var _ = File.Create(fileDst);
                fileCache = new FileWriteCache();
                _writeFiles.TryAdd(fileDst, true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(IFileProvider.PrepareWriteFile));
                cnt--;
                if (cnt == 0)
                    return false;
            }
        }
    }

    void IFileProvider.CompliteReadFile(ushort fileName)
    {
        var fileDst = Path.Combine(_cachePath, string.Format(_readTemplate, fileName, _id2));
        var cnt = 3;

        while (true)
        {
            try
            {
                _readFiles.Remove(fileDst, out _);
                File.Delete(fileDst);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(IFileProvider.CompliteReadFile));
                cnt--;
                if (cnt == 0)
                    return;
            }
        }
    }

    void IFileProvider.CompliteWriteFile(ushort fileName, IEnumerable<byte[]> sections)
    {
        var fileDst = Path.Combine(_cachePath, string.Format(_writeTemplate, fileName, _id2));
        var fileDst2 = Path.Combine(_cachePath, $"{fileName}");

        var cnt = 3;

        while (true)
        {
            try
            {
                _writeFiles.Remove(fileDst, out _);

                using var stream = File.Open(fileDst, FileMode.OpenOrCreate);
                {
                    using var writer = new BinaryWriter(stream, Encoding.UTF8);
                    foreach (var section in sections)
                        writer.Write(section);
                }

                File.Move(fileDst, fileDst2, true);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(IFileProvider.CompliteWriteFile));
                cnt--;
                if (cnt == 0)
                    return;
            }
        }
    }
}

