using System;
using System.Threading;

namespace LabServices.DataTemplates
{
    /// <summary>
    /// Kontener zapeniający obsługę właściwości do której można uzyskać dostęp jedynie w sposób bezpieczny wielowątkowo
    /// </summary>
    /// <typeparam name="T">Typ właściwości</typeparam>
    internal class LockedProperty<T>
    {
        private T _value;
        private readonly object _lock;

        public LockedProperty(T value)
        {
            _value = value;
            _lock = new object();
        }

        public T Get()
        {
            T result;
            lock (_lock)
            {
                result = _value;
            }
            return result;
        }

        public void Set(T newValue)
        {
            lock (_lock)
            {
                _value = newValue;
            }
        }

        /// <summary>
        /// Klasa pozwalająca uzyskać podłączenie dla obiektów wykonujących operacje bezpośrednio na obiekcie np. na liście
        /// Należy używać w bloku > using (var handle = new LockedHandle(property)) { /* Kod */ }
        /// </summary>
        /// <typeparam name="U"></typeparam>
        internal class LockedHandle<U> : IDisposable
        {
            private LockedProperty<U> _property;

            public U Value
            {
                get => _property._value;
                set => _property._value = value;
            }
            internal LockedHandle(LockedProperty<U> property)
            {
                _property = property;
                Monitor.Enter(_property._lock);
            }

            public void Dispose()
            {
                Monitor.Exit(_property._lock);
            }
        }
    }
}
