namespace FreezerM
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
    }
}
