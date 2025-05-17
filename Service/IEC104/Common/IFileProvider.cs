using PowerUnit.Asdu;

namespace PowerUnit;

public record FileSystemItem(ushort FileOrSubDirectoryName, uint FileSize, SOF SOF, DateTime FileOrDirectoryTimeStamp);

public record FileItem(ushort FileOrSubDirectoryName, uint FileSize, SOF SOF, DateTime FileOrDirectoryTimeStamp) : FileSystemItem(FileOrSubDirectoryName, FileSize, SOF, FileOrDirectoryTimeStamp);

public record DirectoryItem(ushort FileOrSubDirectoryName, DateTime FileOrDirectoryTimeStamp) : FileSystemItem(FileOrSubDirectoryName, 0, SOF.IsSubDirectory, FileOrDirectoryTimeStamp);

public class FileWriteCache
{
    public SortedDictionary<byte, List<byte[]>> Sections { get; } = [];
}

public class FileReadCache
{
    public List<(byte[] section, byte cs)> Sections { get; } = [];
    public byte Cs { get; }
    public int Length { get; }

    public FileReadCache(string file, int length, int sectionLength)
    {
        Cs = 0;
        Length = length;
        foreach (var section in File.ReadAllBytes(file).Chunk(sectionLength))
        {
            var sectionCs = section.Aggregate<byte, byte>(0, (x, t) => (byte)(x + t));
            Sections.Add(new(section, sectionCs));
            Cs = (byte)(Cs + sectionCs);
        }
    }
}

public interface IFileProvider
{
    /// <summary>
    /// Установить идентификатор провайдера
    /// </summary>
    /// <param name="id"></param>
    void SetId(int id1, Guid id2);
    /// <summary>
    /// Получить список файлов сервера
    /// </summary>
    /// <param name="address"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    IEnumerable<FileSystemItem> GetDirectoryContent(uint address, ushort fileName);
    /// <summary>
    /// Подготовить чтение файла
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    bool PrepareReadFile(ushort fileName/*, int sectionLength*/, out FileReadCache? fileCache);
    /// <summary>
    /// Подготовить заптсь файла
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileCache"></param>
    /// <returns></returns>
    bool PrepareWriteFile(ushort fileName/*, int sectionLength*/, out FileWriteCache? fileCache);
    /// <summary>
    /// Завершить работу с файлом на чтение
    /// </summary>
    /// <param name="fileName"></param>
    void CompliteReadFile(ushort fileName);
    /// <summary>
    /// Завершить работу с файлом на запись
    /// </summary>
    /// <param name="fileName"></param>
    void CompliteWriteFile(ushort fileName, IEnumerable<byte[]> sections);
}
