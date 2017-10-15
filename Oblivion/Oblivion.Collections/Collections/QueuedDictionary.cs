#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Oblivion.Collections
{
    public class QueuedDictionary<T, TV>
    {
        private ConcurrentQueue<KeyValuePair<T, TV>> _addQueue;
        private ConcurrentQueue<KeyValuePair<T, TV>> _updateQueue;
        private ConcurrentQueue<T> _removeQueue;
        private ConcurrentQueue<OnCycleDoneDelegate> _onCycleEventQueue;
        private EventHandler _onAdd;
        private EventHandler _onUpdate;
        private EventHandler _onRemove;
        private EventHandler _onCycleDone;

        public QueuedDictionary()
        {
            Inner = new ConcurrentDictionary<T, TV>();
            _addQueue = new ConcurrentQueue<KeyValuePair<T, TV>>();
            _updateQueue = new ConcurrentQueue<KeyValuePair<T, TV>>();
            _removeQueue = new ConcurrentQueue<T>();
            _onCycleEventQueue = new ConcurrentQueue<OnCycleDoneDelegate>();
        }

        public QueuedDictionary(EventHandler onAddItem, EventHandler onUpdate, EventHandler onRemove,
            EventHandler onCycleDone)
        {
            Inner = new ConcurrentDictionary<T, TV>();
            _addQueue = new ConcurrentQueue<KeyValuePair<T, TV>>();
            _updateQueue = new ConcurrentQueue<KeyValuePair<T, TV>>();
            _removeQueue = new ConcurrentQueue<T>();
            _onAdd = onAddItem;
            _onUpdate = onUpdate;
            _onRemove = onRemove;
            _onCycleDone = onCycleDone;
            _onCycleEventQueue = new ConcurrentQueue<OnCycleDoneDelegate>();
        }

        public ICollection<TV> Values => Inner.Values;

        public ICollection<T> Keys => Inner.Keys;

        public ConcurrentDictionary<T, TV> Inner { get; set; }

        public void OnCycle()
        {
            WorkRemoveQueue();
            WorkAddQueue();
            WorkUpdateQueue();
            WorkOnEventDoneQueue();
            _onCycleDone?.Invoke(null, new EventArgs());
        }

        public void Add(T key, TV value)
        {
            var keyValuePair = new KeyValuePair<T, TV>(key, value);
            _addQueue.Enqueue(keyValuePair);
        }

        public void Update(T key, TV value)
        {
            var keyValuePair = new KeyValuePair<T, TV>(key, value);
            _updateQueue.Enqueue(keyValuePair);
        }

        public void Remove(T key)
        {
            _removeQueue.Enqueue(key);
        }

        public TV GetValue(T key) => Inner.ContainsKey(key) ? Inner[key] : default(TV);

        public bool ContainsKey(T key) => Inner.ContainsKey(key);

        public void Clear()
        {
            Inner.Clear();
        }

        public void QueueDelegate(OnCycleDoneDelegate function)
        {
            _onCycleEventQueue.Enqueue(function);
        }

        public List<KeyValuePair<T, TV>> ToList() => Inner.ToList();

        public void Destroy()
        {
            Inner?.Clear();
            if (_addQueue != null && _addQueue.Any())
            {
                while (_addQueue.TryDequeue(out _)) { }
            }
            if (_updateQueue != null && _updateQueue.Any())
            {
                while (_updateQueue.TryDequeue(out _)) { }
            }
            if (_removeQueue != null && _removeQueue.Any())
            {
                while (_removeQueue.TryDequeue(out _)) { }
            }
            if (_onCycleEventQueue != null && _onCycleEventQueue.Any())
            {
                while (_onCycleEventQueue.TryDequeue(out _)) { }
            }
            Inner = null;
            _addQueue = null;
            _updateQueue = null;
            _removeQueue = null;
            _onCycleEventQueue = null;
            _onAdd = null;
            _onUpdate = null;
            _onRemove = null;
            _onCycleDone = null;
        }

        private void WorkOnEventDoneQueue()
        {
            if (_onCycleEventQueue.Any())
                return;

            while (_onCycleEventQueue.TryDequeue(out var item))
            {
                item();
            }
        }

        private void WorkAddQueue()
        {
            if (!_addQueue.Any())
                return;
            while (_addQueue.TryDequeue(out var item))
            {
                if (Inner.ContainsKey(item.Key))
                    Inner[item.Key] = item.Value;
                else
                    Inner.TryAdd(item.Key, item.Value);

                _onAdd?.Invoke(item, null);
            }
        }

        private void WorkUpdateQueue()
        {
            if (!_updateQueue.Any())
                return;
            while (_updateQueue.TryDequeue(out var item))
            {
                if (Inner.ContainsKey(item.Key))
                    Inner[item.Key] = item.Value;
                else
                    Inner.TryAdd(item.Key, item.Value);
                _onUpdate?.Invoke(item, null);
            }
        }

        private void WorkRemoveQueue()
        {
            if (!_removeQueue.Any())
                return;
            var list = new List<T>();
            while (_removeQueue.TryDequeue(out var item))
            {
                if (Inner.ContainsKey(item))
                {
                    var value = Inner[item];
                    Inner.TryRemove(item, out _);
                    var keyValuePair = new KeyValuePair<T, TV>(item, value);
                    _onRemove?.Invoke(keyValuePair, null);
                }
                else
                    list.Add(item);
            }
            if (!list.Any())
                return;
            foreach (var current in list)
                _removeQueue.Enqueue(current);
        }
    }
}