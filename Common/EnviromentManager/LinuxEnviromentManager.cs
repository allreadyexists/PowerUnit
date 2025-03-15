namespace PowerUnit.Common.EnviromentManager;

internal sealed class LinuxEnviromentManager(string name) : CommonEnviromentManager(name)
{
    private string _cachePath;
    private const string LINUX_CACHE_DIR = @"/var/cache";
    public override string GetCachePath() => _cachePath ??= Path.Combine(LINUX_CACHE_DIR, Name);

    private string _configPath;
    private const string LINUX_CONFIG_DIR = @"/etc";
    private string ExtractPackageName() => Path.GetFileName(GetDllPath().TrimEnd(Path.DirectorySeparatorChar));
    public override string GetConfigFilesPath() => _configPath ??= Path.Combine(LINUX_CONFIG_DIR, ExtractPackageName());

    private string _dataPath;
    private const string LINUX_SHARE_DIR = @"/usr/share";
    public override string GetDataPath() => _dataPath ??= Path.Combine(LINUX_SHARE_DIR, Name);

    private string _logPath;
    private const string LINUX_LOG_DIR = @"/var/log";

    public override string GetLogPath() => _logPath ??= Path.Combine(LINUX_LOG_DIR, Name);
}
