using PowerUnit.Common.Shell;

using System.Runtime.Versioning;

namespace PowerUnit.Common.DateTimeManager;

internal sealed class LinuxDateTimeManager : IDateTimeManager
{
    /// <summary>
    /// Устанавливает системное время на платформе Linux.
    /// Пользователь, от имени которого выполняются действия, должен быть внесён в файл /etc/sudoers:
    /// username ALL=(ALL) NOPASSWD: /bin/date
    /// username ALL=(ALL) NOPASSWD: /sbin/hwclock
    /// Или должны быть назначены следущие file capabilities:
    /// setcap cap_sys_time+pie /bin/date
    /// setcap cap_sys_time, cap_dac_override+eip /sbin/hwclock
    /// </summary>
    /// <param name="dateTime"></param>
    /// <exception cref="DateTimeManagerException"></exception>
    [SupportedOSPlatform("linux")]
    void IDateTimeManager.SetDateTime(DateTime dateTime)
    {
        try
        {
            // Установка времени ОС.
            var dts = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", default);
            ShellCommand.Execute($"date -s \"{dts}\"");
            // Установка времени RTC по времени ОС.
            ShellCommand.Execute("hwclock --systohc");
        }
        catch (Exception ex)
        {
            throw new DateTimeManagerException(ex.Message, ex);
        }
    }
}
