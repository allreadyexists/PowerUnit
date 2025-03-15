namespace PowerUnit.Common.EnviromentManager;

public interface IEnviromentManager
{
    string GetDllPath();
    string GetLogPath();
    string GetCachePath();
    string GetDataPath();
    string GetConfigFilesPath();
}
