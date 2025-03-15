namespace PowerUnit.Common.Shell;

/// <inheritdoc />
/// <summary>
/// Исключение контроля завершения выполнения консольной команды.
/// </summary>
public class ShellCommandException(string? command, int exitCode, string? message) : Exception($"{command}\n{message}")
{
    /// <summary>
    /// Код завершения выполнения команды.
    /// </summary>
    public int ExitCode { get; } = exitCode;
}
