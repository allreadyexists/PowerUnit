using System.Reflection;

namespace PowerUnit.Common.EnviromentManager;

internal abstract class CommonEnviromentManager : IEnviromentManager
{
    public string Name { get; }

    public CommonEnviromentManager(string name)
    {
        Name = name;
        var logPath = GetLogPath();
        if (!Directory.Exists(logPath))
            Directory.CreateDirectory(logPath);
        var cachePath = GetCachePath();
        if (!Directory.Exists(cachePath))
            Directory.CreateDirectory(cachePath);
        var dataPath = GetDataPath();
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        var configPath = GetConfigFilesPath();
        if (!Directory.Exists(configPath))
            Directory.CreateDirectory(configPath);
    }

    private static string? AddSlashToPath(string? path)
    {
        return string.IsNullOrEmpty(path) || path[^1] == Path.DirectorySeparatorChar
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    private string DllPath { get; set; }

    public string GetDllPath()
    {
        try
        {
            return DllPath ??= AddSlashToPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public abstract string GetLogPath();
    public abstract string GetCachePath();
    public abstract string GetDataPath();
    public abstract string GetConfigFilesPath();
}
