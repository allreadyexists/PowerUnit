namespace PowerUnit.Common.EnviromentManager;

internal sealed class WindowsEnviromentManager(string name) : CommonEnviromentManager(name)
{
    private const string WINDOWS_CACHE_SUBDIR = "Cache";
    private string _caсhePath;

    public override string GetCachePath() => _caсhePath ??= Path.Combine(GetDllPath(), WINDOWS_CACHE_SUBDIR, Name);

    public override string GetConfigFilesPath() => GetDllPath();

    private const string WINDOWS_DATA_SUBDIR = "Data";
    private string _dataPath;

    public override string GetDataPath() => _dataPath ??= Path.Combine(GetDllPath(), WINDOWS_DATA_SUBDIR, Name);

    private const string WINDOWS_LOG_SUBDIR = "Logs";
    private string _logsPath;

    public override string GetLogPath() => _logsPath ??= Path.Combine(GetDllPath(), WINDOWS_LOG_SUBDIR, Name);
}
