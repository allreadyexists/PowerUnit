namespace PowerUnit.Common.Exceptions;

/// <summary>
/// Исключение, используемое для технических нужд разработки. 
/// Никогда не должно проявляться у заказчика и не предполагает специального catch-обработчика
/// Текст исключения не требует локализации и может быть хардкодно указан в конструкторе на русском языке
/// </summary>
public class TechnicalException : Exception
{
    /// <summary>
    /// Создание с инициализацией
    /// </summary>
    /// <param name="message"></param>
    public TechnicalException(string? message)
        : base(message)
    {

    }
}
