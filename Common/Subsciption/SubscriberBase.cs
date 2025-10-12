using PowerUnit.Common.Other;

namespace PowerUnit.Common.Subsciption;

public abstract class SubscriberBase<T, TContext> : IDisposable
{
    public Guid Id { get; private set; }

    protected IDisposable? Subscribe { get; set; }
    protected IDataSource<T> DataSource { get; set; }
    protected TContext Context { get; set; }
    protected CancellationTokenSource TokenSource { get; set; }

    protected Action<Exception> OnError { get; set; }
    protected Action OnComplite { get; set; }
    protected Func<T, TContext, bool> Filter { get; set; }
    protected ISubscriberDiagnostic? SubscriberDiagnostic { get; }

    protected string TypeName { get; }

    public SubscriberBase(IDataSource<T> dataSource, TContext context, Action<Exception>? onError, Action? onComplite, Func<T, TContext, bool>? filter,
        ISubscriberDiagnostic? subscriberDiagnostic)
    {
        Id = Guid.NewGuid();
        TypeName = GetType().GetFormattedName();

        DataSource = dataSource;
        Context = context;
        OnError = onError ?? DelegateHelper.Empty<Exception>();
        OnComplite = onComplite ?? DelegateHelper.Empty();
        Filter = filter ?? (static (value, context) => true);
        SubscriberDiagnostic = subscriberDiagnostic;

        TokenSource = new CancellationTokenSource();
    }

    void IDisposable.Dispose()
    {
        Subscribe?.Dispose();
        TokenSource.Cancel();
        TokenSource.Dispose();
        (Context as IDisposable)?.Dispose();
    }
}
