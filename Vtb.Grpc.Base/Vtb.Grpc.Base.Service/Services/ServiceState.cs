namespace Vtb.Grpc.Base.Service
{
    using System;
    using System.Collections.Concurrent;

    public interface IStateObserver<T> : IObserver<T>
    {
        public string Identity { get; }
    }

    public class ServiceState<T> : IObservable<T>, IObserver<T>
    {
        public ServiceState()
        {
            //m_state = default(T);
            m_subsribers = new ConcurrentDictionary<string, Subscribtion<T>>();
        }

        public void OnCompleted()
        {
            foreach (var subscriber in m_subsribers)
            {
                subscriber.Value.Observer.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(T value)
        {
            foreach (var subscriber in m_subsribers)
            {
                subscriber.Value.Observer.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var o = (IStateObserver<T>)observer;
            return m_subsribers.GetOrAdd(o.Identity, 
                key => new Subscribtion<T>(() => UnSubscribe(key), observer));
        }

        private void UnSubscribe(string identity)
        {
            if (m_subsribers.TryRemove(identity, out var subscribtion))
            {
                subscribtion.Observer.OnCompleted();
            }
        }

        private class Subscribtion<TT> : IDisposable
        {
            private Action OnDispose;
            public IObserver<TT> Observer { get; }

            public Subscribtion(Action onDispose, IObserver<TT> observer )
            {
                OnDispose = onDispose;
                Observer = observer;
            }

            public void Dispose()
            {
                OnDispose?.Invoke();
                OnDispose = null;
            }
        }

        //private T m_state;
        private ConcurrentDictionary<string, Subscribtion<T>> m_subsribers;
    }
}
