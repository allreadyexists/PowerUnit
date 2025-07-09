using PowerUnit.Common.DateTimeManager.Properties;

using System.Management;
using System.Runtime.Versioning;

namespace PowerUnit.Common.DateTimeManager;

internal sealed class WindowsDateTimeManager : IDateTimeManager
{
    /// <summary>
    /// Устанавливает системное время на платформе Windows.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <exception cref="DateTimeManagerException"></exception>
    [SupportedOSPlatform("windows")]
    void IDateTimeManager.SetDateTime(DateTime dateTime)
    {
        try
        {
            var scope = new ManagementScope(@"\\.\root\cimv2");
            var path = new ManagementPath("Win32_OperatingSystem=@");
            var mClass = new ManagementObject(scope, path, null);
            var inParams = mClass.GetMethodParameters("SetDateTime");
            inParams["LocalDateTime"] = ManagementDateTimeConverter.ToDmtfDateTime(dateTime);
            var outParams = mClass.InvokeMethod("SetDateTime", inParams, null);
            if (outParams == null || Convert.ToUInt32(outParams.Properties["ReturnValue"].Value, default) != 0u)
                throw new DateTimeManagerException(Resources.UnknownDateTimeSettingError);
        }
        catch (ManagementException ex)
        {
            throw new DateTimeManagerException(ex.Message, ex);
        }
    }
}
