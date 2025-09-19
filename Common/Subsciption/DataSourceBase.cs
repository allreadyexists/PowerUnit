using Microsoft.Extensions.Logging;

using System.Reactive.Subjects;

namespace PowerUnit.Common.Subsciption;

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
        if (_subject.HasObservers)
            _subject.OnNext(value);
    }

    protected void Notify(Func<DataSourceBase<T>, T> generatorValue)
    {
        if (_subject.HasObservers)
        {
            var value = generatorValue(this);
            _subject.OnNext(value);
        }
    }

    public abstract void Dispose();
}
