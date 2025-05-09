using PowerUnit.Asdu;

namespace PowerUnit;

public record FileSystemItem(ushort FileOrSubDirectoryName, uint FileSize, SOF SOF, DateTime FileOrDirectoryTimeStamp);

public record FileItem(ushort FileOrSubDirectoryName, uint FileSize, SOF SOF, DateTime FileOrDirectoryTimeStamp) : FileSystemItem(FileOrSubDirectoryName, FileSize, SOF, FileOrDirectoryTimeStamp);

public record DirectoryItem(ushort FileOrSubDirectoryName, DateTime FileOrDirectoryTimeStamp) : FileSystemItem(FileOrSubDirectoryName, 0, SOF.IsSubDirectory, FileOrDirectoryTimeStamp);

public interface IFileProvider
{
    /// <summary>
    /// Получить список файлов сервера
    /// </summary>
    /// <param name="address"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    IEnumerable<FileSystemItem> GetDirectoryContent(uint address, ushort fileName);
    /// <summary>
    /// Получить информацию о файле
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    FileInfo? GetFileInfo(ushort fileName);
    /// <summary>
    /// Получить секцию файла
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sectionName"></param>
    /// <param name="sectionMaxLength"></param>
    /// <returns></returns>
    byte[] GetSection(ushort fileName, byte sectionName, uint sectionMaxLength);
    /// <summary>
    /// Завершить работу с файлом
    /// </summary>
    /// <param name="fileName"></param>
    void CloseFile(ushort fileName);
}
