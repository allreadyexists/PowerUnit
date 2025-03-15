namespace PowerUnit.Common.Exceptions;

/// <summary>
/// Исключение, предназначенное для отлова на этапе разработки.
/// Например, в случае несогласованной design-time конфигурации, 
/// без которой ничего не заработает.
/// Типичный случай - не настроенный IoC
/// </summary>
public class DesignTimeException : TechnicalException
{
    /// <summary>
    /// Создание с инициализацией
    /// </summary>
    /// <param name="message"></param>
    public DesignTimeException(string? message)
        : base(message)
    {

    }
}
