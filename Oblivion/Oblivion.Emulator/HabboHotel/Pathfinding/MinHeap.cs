using System;
using System.Collections.Generic;

namespace Oblivion.HabboHotel.PathFinding
{
    /// <summary>
    ///     Class MinHeap.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MinHeap<T> where T : IComparable<T>
    {
        /// <summary>
        ///     The _array
        /// </summary>
        private T[] _array;

        /// <summary>
        ///     The _capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     The _m heap
        /// </summary>
        private T _mHeap;

        /// <summary>
        ///     The _temp
        /// </summary>
        private T _temp;

        /// <summary>
        ///     The _temp array
        /// </summary>
        private T[] _tempArray;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinHeap{T}" /> class.
        /// </summary>
        public MinHeap() : this(16)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinHeap{T}" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public MinHeap(int capacity)
        {
            Count = 0;
            _capacity = capacity;
            _array = new T[capacity];
        }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; private set; }

        /// <summary>
        ///     Builds the head.
        /// </summary>
        public void BuildHead()
        {
            for (var i = Count - 1 >> 1; i >= 0; i--)
                MinHeapify(i);
        }

        /// <summary>
        ///     Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            Count++;

            if (Count > _capacity)
                DoubleArray();

            _array[Count - 1] = item;

            var num = Count - 1;
            var num2 = num - 1 >> 1;

            while (num > 0 && _array[num2].CompareTo(_array[num]) > 0)
            {
                _temp = _array[num];
                _array[num] = _array[num2];
                _array[num2] = _temp;
                num = num2;
                num2 = num - 1 >> 1;
            }
        }

        /// <summary>
        ///     Peeks this instance.
        /// </summary>
        /// <returns>T.</returns>
        /// <exception cref="System.InvalidOperationException">Heap is empty</exception>
        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty");

            return _array[0];
        }

        /// <summary>
        ///     Extracts the first.
        /// </summary>
        /// <returns>T.</returns>
        /// <exception cref="System.InvalidOperationException">Heap is empty</exception>
        public T ExtractFirst()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty");

            _temp = _array[0];

            _array[0] = _array[Count - 1];
            Count--;
            MinHeapify(0);

            return _temp;
        }

        /// <summary>
        ///     Copies the array.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        private static void CopyArray(IList<T> source, IList<T> destination)
        {
            for (var i = 0; i < source.Count; i++)
                destination[i] = source[i];
        }

        /// <summary>
        ///     Doubles the array.
        /// </summary>
        private void DoubleArray()
        {
            _capacity <<= 1;
            _tempArray = new T[_capacity];
            CopyArray(_array, _tempArray);
            _array = _tempArray;
        }

        /// <summary>
        ///     Minimums the heapify.
        /// </summary>
        /// <param name="position">The position.</param>
        private void MinHeapify(int position)
        {
            while (true)
            {
                var num = (position << 1) + 1;
                var num2 = num + 1;
                int num3;

                if (num < Count && _array[num].CompareTo(_array[position]) < 0)
                    num3 = num;
                else
                    num3 = position;

                if (num2 < Count && _array[num2].CompareTo(_array[num3]) < 0)
                    num3 = num2;

                if (num3 == position)
                    break;

                _mHeap = _array[position];
                _array[position] = _array[num3];
                _array[num3] = _mHeap;
                position = num3;
            }
        }
    }
}