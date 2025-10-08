using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

namespace PowerUnit.Common.Reactive;

internal static class Buffer2<TSource>
{
    internal sealed class Ferry2 : Producer<IList<TSource>, Ferry2._>
    {
        private readonly IObservable<TSource> _source;
        private readonly int _count;
        private readonly TimeSpan _timeSpan;
        private readonly IScheduler _scheduler;

        public Ferry2(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
        {
            _source = source;
            _timeSpan = timeSpan;
            _count = count;
            _scheduler = scheduler;
        }

        protected override _ CreateSink(IObserver<IList<TSource>> observer) => new(this, observer);

        protected override void Run(_ sink) => sink.Run();

        internal sealed class _ : Sink<TSource, IList<TSource>>
        {
            private readonly Ferry2 _parent;
            private readonly int _count;
#pragma warning disable IDE0330 // Use 'System.Threading.Lock'
            private readonly object _gate = new();
#pragma warning restore IDE0330 // Use 'System.Threading.Lock'
            private List<TSource> _s;

            public _(Ferry2 parent, IObserver<IList<TSource>> observer)
                : base(observer)
            {
                _parent = parent;
                _count = _parent._count;
                _s = new List<TSource>(_count);
            }

            private SerialDisposableValue _timerSerial;
            private int _n;
            private int _windowId;

            public void Run()
            {
                _n = 0;
                _windowId = 0;

                CreateTimer(0);

                SetUpstream(_parent._source.SubscribeSafe(this));
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timerSerial.Dispose();
                }

                base.Dispose(disposing);
            }

            private void CreateTimer(int id)
            {
                var m = new SingleAssignmentDisposable();
                _timerSerial.Disposable = m;

                m.Disposable = _parent._scheduler.ScheduleAction((@this: this, id), _parent._timeSpan, static tuple => tuple.@this.Tick(tuple.id));
            }

            private void Tick(int id)
            {
                lock (_gate)
                {
                    if (id != _windowId)
                    {
                        return;
                    }

                    var res = _s;

                    CollectionsMarshal.SetCount(res, _n);

                    _n = 0;
                    var newId = ++_windowId;

                    _s = new List<TSource>(_count);
                    ForwardOnNext(res);

                    CreateTimer(newId);
                }
            }

            public override void OnNext(TSource value)
            {
                var newWindow = false;
                var newId = 0;

                lock (_gate)
                {
                    _s.Add(value);
                    _n++;

                    if (_n == _count)
                    {
                        newWindow = true;
                        _n = 0;
                        newId = ++_windowId;

                        var res = _s;
                        _s = new List<TSource>(_count);
                        ForwardOnNext(res);
                    }

                    if (newWindow)
                    {
                        CreateTimer(newId);
                    }
                }
            }

            public override void OnError(Exception error)
            {
                lock (_gate)
                {
                    _s.Clear();
                    ForwardOnError(error);
                }
            }

            public override void OnCompleted()
            {
                lock (_gate)
                {
                    ForwardOnNext(_s);
                    ForwardOnCompleted();
                }
            }
        }
    }
}
