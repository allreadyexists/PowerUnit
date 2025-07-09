using PowerUnit.Common.Subsciption;

using System.Reactive.Subjects;

namespace PowerUnit.Service.IEC104.Export;

public abstract class DataSourceBase<T> : IDataSource<T>, IDisposable
{
    protected ILogger<DataSourceBase<T>> Logger { get; }

    private readonly Subject<T> _subject = new Subject<T>();

    public DataSourceBase(ILogger<DataSourceBase<T>> logger)
    {
        Logger = logger;
    }

    IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
    {
        return _subject.Subscribe(observer);
    }

    protected void Notify(T value)
    {
        _subject.OnNext(value);
    }

    public abstract void Dispose();
}
