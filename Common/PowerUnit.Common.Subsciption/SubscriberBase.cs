namespace PowerUnit.Common.Subsciption;

public abstract class SubscriberBase<T> : IDisposable
{
    public Guid Id { get; private set; }

    protected IDisposable Subscribe { get; set; }
    protected IDataSource<T> DataSource { get; set; }
    protected CancellationTokenSource TokenSource { get; set; }

    protected Action<Exception> OnError { get; set; }
    protected Action OnComplite { get; set; }
    protected Predicate<T> Filter { get; set; }

    public SubscriberBase(IDataSource<T> dataSource, Action<Exception>? onError, Action? onComplite, Predicate<T>? filter)
    {
        Id = Guid.NewGuid();

        DataSource = dataSource;
        OnError = onError ?? DelegateHelper.Empty<Exception>();
        OnComplite = onComplite ?? DelegateHelper.Empty();
        Filter = filter ?? (static (value) => true);

        TokenSource = new CancellationTokenSource();
    }

    void IDisposable.Dispose()
    {
        Subscribe.Dispose();
        TokenSource.Cancel();
        TokenSource.Dispose();
    }
}
