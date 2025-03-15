namespace PowerUnit.Common.Exceptions;

/// <summary>
/// Исключение, которое по логике работы программы никогда не должно появиться
/// Используется, в частности, для согласования
/// </summary>
public class NeverRaiseException : TechnicalException
{
    /// <summary>
    /// Создание с инициализацией
    /// </summary>
    /// <param name="message"></param>
    public NeverRaiseException(string? message)
        : base(message)
    {

    }
}
