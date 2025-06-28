using System.Collections.Concurrent;

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

    //private readonly ConcurrentDictionary<string, bool> _readFiles = [];
    private readonly ConcurrentDictionary<string, bool> _writeFiles = [];

    private readonly ConcurrentDictionary<ushort, FileReaderState> _fileBinaryReaders = new ConcurrentDictionary<ushort, FileReaderState>();
    private readonly ConcurrentDictionary<ushort, FileWriterState> _fileBinaryWriters = new ConcurrentDictionary<ushort, FileWriterState>();

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

        foreach (var fileBinaryReader in _fileBinaryReaders)
        {
            fileBinaryReader.Value.Reader.Close();
            fileBinaryReader.Value.Reader.Dispose();
        }

        foreach (var fileBinaryWriter in _fileBinaryWriters)
        {
            fileBinaryWriter.Value.Writer.Close();
            fileBinaryWriter.Value.Writer.Dispose();
        }
    }

    void IFileProvider.SetId(int id1)
    {
        if (_id1 != null)
            throw new ArgumentException(nameof(IFileProvider.SetId), nameof(id1));

        _id1 = id1;
        _id2 = Guid.NewGuid();
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
                yield return new FileSystemItem(fileNameAsNumber, fileInfo.Length, 0, fileInfo.CreationTimeUtc);
            }
        }
    }

    bool IFileProvider.PrepareReadFile(ushort fileName, out FileReaderState? readState)
    {
        var cnt = 3;
        readState = null;
        while (true)
        {
            try
            {
                var fileSrc = Path.Combine(_cachePath, $"{fileName}");
                var fileDst = Path.Combine(_cachePath, string.Format(_readTemplate, fileName, _id2));

                if (!File.Exists(fileSrc) && !File.Exists(fileDst))
                    return false;

                if (_fileBinaryReaders.ContainsKey(fileName))
                    return false;

                if (!File.Exists(fileDst))
                    File.Move(fileSrc, fileDst);

                readState = new FileReaderState(fileDst, fileSrc, new BinaryReader(File.OpenRead(fileDst)), 0);

                _fileBinaryReaders.TryAdd(fileName, readState);
                return true;
            }
            catch (Exception ex)
            {
                if (readState != null)
                {
                    readState.Reader.Dispose();
                }

                _logger.LogError(ex, nameof(IFileProvider.PrepareReadFile));
                cnt--;
                if (cnt == 0)
                    return false;
            }
        }
    }

    bool IFileProvider.PrepareWriteFile(ushort fileName, out FileWriterState? writeState)
    {
        var cnt = 3;
        writeState = null;
        while (true)
        {
            try
            {
                var fileSrc = Path.Combine(_cachePath, $"{fileName}");
                var fileDst = Path.Combine(_cachePath, string.Format(_writeTemplate, fileName, _id2));

                if (!File.Exists(fileSrc) && !File.Exists(fileDst))
                    return false;

                if (_fileBinaryWriters.ContainsKey(fileName))
                    return false;

                if (!File.Exists(fileDst))
                    File.Move(fileSrc, fileDst);

                writeState = new FileWriterState(fileDst, fileSrc, new BinaryWriter(File.OpenWrite(fileDst)), 0);

                _fileBinaryWriters.TryAdd(fileName, writeState);
                return true;
            }
            catch (Exception ex)
            {
                if (writeState != null)
                {
                    writeState.Writer.Dispose();
                }

                _logger.LogError(ex, nameof(IFileProvider.PrepareWriteFile));
                cnt--;
                if (cnt == 0)
                    return false;
            }
        }
    }

    void IFileProvider.CompliteReadFile(ushort fileName)
    {
        var cnt = 3;

        while (true)
        {
            try
            {
                if (_fileBinaryReaders.Remove(fileName, out var fileReaderState))
                {
                    File.Move(fileReaderState.Name, fileReaderState.OriginName);
                }

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

    void IFileProvider.CompliteWriteFile(ushort fileName)
    {
        var cnt = 3;

        while (true)
        {
            try
            {
                if (_fileBinaryWriters.Remove(fileName, out var fileWriterState))
                {
                    File.Move(fileWriterState.Name, fileWriterState.OriginName);
                }

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

