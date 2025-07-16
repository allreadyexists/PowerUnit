using System.Collections.Concurrent;

namespace PowerUnit.Service.IEC104.Application;

/// <summary>
/// Отслеживание выполняемых запросов на чтение данных
/// индивидуален в рамках соединения
/// </summary>
public class ApplicationLayerReadTransactionManager : IDisposable
{
    /// <summary>
    /// Запущенные транзакции в рамках сессии
    /// </summary>
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _transactionIds = new();

    /// <summary>
    /// Создать транзакцию - идентификатор передает клиент
    /// т.к. только он знает как его получить для конкретного типа запроса
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    public bool CreateTransaction(int transactionId, out CancellationToken ct)
    {
        var cts = new CancellationTokenSource();
        ct = cts.Token;
        return _transactionIds.TryAdd(transactionId, cts);
    }

    public bool DeleteTransaction(int transactionId)
    {
        var remove = _transactionIds.TryRemove(transactionId, out var cts);
        if (remove)
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        return remove;
    }

    void IDisposable.Dispose()
    {
        foreach (var transactionId in _transactionIds)
        {
            transactionId.Value.Dispose();
        }
    }
}

