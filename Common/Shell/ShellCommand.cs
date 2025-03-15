using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerUnit.Common.Shell;

/// <summary>
/// Класс выполнения консольных команд.
/// </summary>
/// <remarks>
/// Поддерживаются все платформы.
/// </remarks>
public static class ShellCommand
{
    /// <summary>
    /// Код успешного выполнения команды.
    /// </summary>
    public const int SUCCESS = 0;

    /// <summary>
    /// Запускает на выполнение консольную команду в безоконном режиме и дожидается её завершения.
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <exception cref="ShellCommandProcessException">Не удалось запустить команду на выполнение.</exception>
    /// <exception cref="ShellCommandException">Выполнение команды завершено с ошибкой.</exception>
    public static void Execute(string command) => Execute(command, out _);

    /// <summary>
    /// Запускает на выполнение консольную команду в безоконном режиме и дожидается её завершения.
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <param name="output">Содержимое STDOUT.</param>
    /// <exception cref="ShellCommandProcessException">Не удалось запустить команду на выполнение.</exception>
    /// <exception cref="ShellCommandException">Выполнение команды завершено с ошибкой.</exception>
    public static void Execute(string command, out string? output) => ExecuteInternal(command, out output, out _, null);

    /// <summary>
    /// Запускает на выполнение консольную команду в безоконном режиме и дожидается её завершения.
    /// Вывод с поддержкой UTF-8
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <param name="output">Содержимое STDOUT.</param>
    /// <exception cref="ShellCommandProcessException">Не удалось запустить команду на выполнение.</exception>
    /// <exception cref="ShellCommandException">Выполнение команды завершено с ошибкой.</exception>
    public static void ExecuteUtf8(string command, out string? output)
    {
        ExecuteInternal(command, out output, out _, x =>
        {
            x.StandardOutputEncoding = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Encoding.UTF8 : Encoding.Default;
        });
    }

    /// <summary>
    /// Запускает на выполнение консольную команду в безоконном режиме и дожидается её завершения.
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <param name="output">Содержимое STDOUT.</param>
    /// <param name="error">Содержимое STDERR.</param>
    /// <exception cref="ShellCommandProcessException">Не удалось запустить команду на выполнение.</exception>
    /// <exception cref="ShellCommandException">Выполнение команды завершено с ошибкой.</exception>
    public static void Execute(string command, out string? output, out string? error)
    {
        ExecuteInternal(command, out output, out error, null);
    }

    /// <summary>
    /// Выполнить команду с выводом в консоль
    /// </summary>
    /// <param name="command"></param>
    public static void ExecuteCommandWithConsoleOutput(string command)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var processInfo = new ProcessStartInfo
        {
            FileName = isWindows
                ? "cmd.exe"
                : "/bin/bash",
            Arguments = isWindows
                ? "/c \"" + WinEscapeSpecialChars(command) + "\""
                : "-c \"" + command.Replace("\"", "\\\"") + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(processInfo);
        {
            if (process == null)
                throw new ShellCommandProcessException();
            var errorText = new StringBuilder();
            int exitCode;
            try
            {
                process.OutputDataReceived += (sender, e) =>
                    Console.WriteLine(e.Data);
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (sender, e) =>
                    errorText.AppendLine(e.Data);
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            finally
            {
                process.Close();
            }

            if (exitCode != SUCCESS)
                throw new ShellCommandException(command, process.ExitCode, errorText.ToString());
        }
    }

    /// <summary>
    /// Обрамляет служебные символы в команде, передаваемой в качестве параметра CMD.EXE.
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <return>Обработанная команда.</return>
    private static string WinEscapeSpecialChars(string command)
    {
        const string PIPE_CHARS = @"&|<>";
        const string SPECIAL_CHARS = @"^";

        var sb = new StringBuilder();
        var innerQuotes = false;
        foreach (var ch in command)
        {
            if (ch == '"')
                innerQuotes = !innerQuotes;
            if (PIPE_CHARS.Contains(ch) || innerQuotes && SPECIAL_CHARS.Contains(ch))
                sb.Append('^');
            sb.Append(ch);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Запускает на выполнение консольную команду в безоконном режиме и дожидается её завершения.
    /// </summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <param name="output">Содержимое STDOUT.</param>
    /// <param name="error">Содержимое STDERR.</param>
    /// <param name="configAction">Опциональная процедура настройки</param>
    /// <exception cref="ShellCommandProcessException">Не удалось запустить команду на выполнение.</exception>
    /// <exception cref="ShellCommandException">Выполнение команды завершено с ошибкой.</exception>
    private static void ExecuteInternal(string command, out string? output, out string? error, Action<ProcessStartInfo>? configAction)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var processInfo = new ProcessStartInfo
        {
            FileName = isWindows
                ? "cmd.exe"
                : "/bin/bash",
            Arguments = isWindows
                ? "/c \"" + WinEscapeSpecialChars(command) + "\""
                : "-c \"" + command.Replace("\"", "\\\"") + "\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        configAction?.Invoke(processInfo);
        using var process = Process.Start(processInfo);
        {
            if (process == null)
                throw new ShellCommandProcessException();
            int exitCode;
            var errorText = new StringBuilder();
            var outputText = new StringBuilder();
            try
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    outputText.AppendLine(e.Data);
                };
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        errorText.AppendLine(e.Data);
                };
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            finally
            {
                process.Close();
            }

            if (exitCode != SUCCESS)
                throw new ShellCommandException(command, exitCode, errorText.ToString());

            output = outputText.ToString();
            error = errorText.ToString();
        }
    }
}
