using System.Reflection;

namespace PowerUnit.Common.Exceptions;

/// <summary>
/// Утилиты для работы с исключениями
/// </summary>
public static class ExceptionHelpers
{
    /// <summary>
    /// Вернуть все вложенные исключения одного уровня
    /// </summary>
    /// <param name="exception"></param>

    private static IEnumerable<Exception> GetInnerExceptions(Exception exception)
    {
        var innerException = exception.InnerException;
        if (innerException != null)
            yield return innerException;
        var loaderException = exception as ReflectionTypeLoadException;
        if (loaderException != null && loaderException.LoaderExceptions != null)
        {
            foreach (var innerLoaderException in loaderException.LoaderExceptions.Where(e => e != null))
            {
                yield return innerLoaderException!;
            }
        }
    }

    /// <summary>
    /// Преобразовать исключение в группу более подробных исключений (более полезных для логирования)
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>

    public static IEnumerable<Exception> EnumInnerExceptions(this Exception exception)
    {
        yield return exception;
        foreach (var innerException in GetInnerExceptions(exception).SelectMany(EnumInnerExceptions))
            yield return innerException;
    }

    /// <summary>
    /// Преобразовать исключение в строку более подробных исключений (более полезных для логирования)
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static string? GetInnerExceptionsString(this Exception exception)
    {
        return string.Join(";\n", exception.EnumInnerExceptions().Select((x, i) => $"{i}. {x.GetType()} - {x.Message} - {x.StackTrace}"));
    }
}
