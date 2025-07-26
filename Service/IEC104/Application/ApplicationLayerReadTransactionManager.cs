using System.Collections.Concurrent;

namespace PowerUnit;

/// <summary>
/// Отслеживание выполняемых запросов на чтение данных
/// индивидуален в рамках соединения
/// </summary>
internal sealed class ApplicationLayerReadTransactionManager : IDisposable
{
    /// <summary>
    /// Запущенные транзакции в рамках сессии
    /// </summary>
    private readonly ConcurrentDictionary<int, bool> _transactionIds = new();

    /// <summary>
    /// Создать транзакцию - идентификатор передает клиент
    /// т.к. только он знает как его получить для конкретного типа запроса
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    public bool CreateTransaction(int transactionId)
    {
        return _transactionIds.TryAdd(transactionId, true);
    }

    public bool DeleteTransaction(int transactionId)
    {
        return _transactionIds.TryRemove(transactionId, out var _);
    }

    void IDisposable.Dispose()
    {
    }
}

