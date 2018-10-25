// Developed for LilBytes by Softeq Development Corporation
//

using System.Threading;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    internal class ThreadSafe<T>
    {
        private readonly ReaderWriterLockSlim _lock;

        private T _value;

        public ThreadSafe(T initialValue)
        {
            _lock = new ReaderWriterLockSlim();

            _value = initialValue;
        }

        public ThreadSafe<T> Set(T newValue)
        {
            _lock.EnterWriteLock();

            _value = newValue;

            _lock.ExitWriteLock();

            return this;
        }

        public T Get()
        {
            _lock.EnterReadLock();

            var result = _value;

            _lock.ExitReadLock();

            return result;
        }
    }
}
