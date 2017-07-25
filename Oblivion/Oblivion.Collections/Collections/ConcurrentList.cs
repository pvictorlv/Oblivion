using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Oblivion.Collections
{
    public sealed class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> _inner;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ConcurrentList() => _inner = new List<T>();

        public ConcurrentList(int capacity) => _inner = new List<T>(capacity);

        public ConcurrentList(IEnumerable<T> original) => _inner = new List<T>(original);

        public int Count
        {
            get
            {
                using (_lock.ReadLock())
                {
                    return _inner.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                using (_lock.ReadLock())
                {
                    return _inner[index];
                }
            }
            set
            {
                using (_lock.WriteLock())
                {
                    _inner[index] = value;
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            using (_lock.ReadLock())
            {
                return new List<T>(_inner).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            using (_lock.ReadLock())
            {
                return new List<T>(_inner).GetEnumerator();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            using (_lock.ReadLock())
            {
                return new List<T>(_inner).GetEnumerator();
            }
        }

        public void Add(T item)
        {
            using (_lock.WriteLock())
            {
                _inner.Add(item);
            }
        }

        public void Clear()
        {
            using (_lock.WriteLock())
            {
                _inner.Clear();
            }
        }

        public bool Contains(T item)
        {
            using (_lock.ReadLock())
            {
                return _inner.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (_lock.ReadLock())
            {
                _inner.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            using (_lock.WriteLock())
            {
                return _inner.Remove(item);
            }
        }


        public int IndexOf(T item)
        {
            using (_lock.ReadLock())
            {
                return _inner.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            using (_lock.WriteLock())
            {
                _inner.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            using (_lock.WriteLock())
            {
                _inner.RemoveAt(index);
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            using (_lock.ReadLock())
            {
                return new ReadOnlyCollection<T>(this);
            }
        }

        public void ForEach(Action<T> action)
        {
            using (_lock.WriteLock())
            {
                foreach (var item in _inner)
                    action(item);
            }
        }

        public bool Exists(Predicate<T> match)
        {
            using (_lock.ReadLock())
            {
                if (_inner.Any(item => match(item)))
                    return true;
            }

            return false;
        }
    }

}