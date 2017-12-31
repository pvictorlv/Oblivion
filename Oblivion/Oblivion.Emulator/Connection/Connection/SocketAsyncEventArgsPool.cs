using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Oblivion.Connection.Connection
{
    internal sealed class SocketAsyncEventArgsPool
    {
        //just for assigning an ID so we can watch our objects while testing.
        private int nextTokenId;

        // Pool of reusable SocketAsyncEventArgs objects.        
        private readonly Stack<SocketAsyncEventArgs> pool;

        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        internal SocketAsyncEventArgsPool(int capacity) => pool = new Stack<SocketAsyncEventArgs>(capacity);

        // The number of SocketAsyncEventArgs instances in the pool.         
        internal int Count => pool.Count;

        internal int AssignTokenId()
        {
            var tokenId = Interlocked.Increment(ref nextTokenId);
            return tokenId;
        }

        // Removes a SocketAsyncEventArgs instance from the pool.
        // returns SocketAsyncEventArgs removed from the pool.
        internal SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                return pool.Pop();
            }
        }

        // Add a SocketAsyncEventArg instance to the pool. 
        // "item" = SocketAsyncEventArgs instance to add to the pool.
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            lock (pool)
            {
                pool.Push(item);
            }
        }
    }
}