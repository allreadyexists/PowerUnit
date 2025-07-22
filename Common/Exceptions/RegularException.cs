namespace PowerUnit.Common.Exceptions;

public class RegularException : Exception
{
    /// <summary>
    /// Создание с инициализацией
    /// </summary>
    /// <param name="message"></param>
    public RegularException(string? message)
        : base(message)
    {

    }
}
