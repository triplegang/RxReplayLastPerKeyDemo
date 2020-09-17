using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxReplayLastPerKeyDemo
{
    public class ReplayLastPerKeySubject<T, TKey> : ISubject<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly ReplaySubject<ReplaySubject<T>> _subjects;
        private readonly IObservable<T> _mergedSubjects;
        private readonly Dictionary<TKey, ReplaySubject<T>> _perKey;

        private readonly object _gate = new object();

        public ReplayLastPerKeySubject(Func<T, TKey> keySelector)
        {
            _keySelector = keySelector;
            _subjects = new ReplaySubject<ReplaySubject<T>>();
            _mergedSubjects = _subjects.Merge();
            _perKey = new Dictionary<TKey, ReplaySubject<T>>();
        }

        public void OnNext(T value)
        {
            lock (_gate)
            {
                var key = _keySelector(value);
                if (!_perKey.TryGetValue(key, out ReplaySubject<T> subject))
                {
                    subject = new ReplaySubject<T>(1);
                    _perKey.Add(key, subject);
                    _subjects.OnNext(subject);
                }
                subject.OnNext(value);
            }
        }

        public void OnCompleted()
        {
            lock (_gate)
            {
                // All subjects, inner and outer, must be completed
                foreach (var subject in _perKey.Values)
                {
                    subject.OnCompleted();
                }
                _subjects.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            lock (_gate)
            {
                // Faulting the master (outer) subject is enough
                _subjects.OnError(error);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _mergedSubjects.Subscribe(observer);
        }
    }
}
