using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Wist.Network.Communication
{
    ///<summary>Represents a collection of reusable SocketAsyncEventArgs objects.</summary>
    internal class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> _pool;

        private Int32 _nextTokenId = 0;

        /// <summary>
        /// Initializes the object pool to the specified size
        ///
        /// The "capacity" parameter is the maximum number of 
        /// SocketAsyncEventArgs objects the pool can hold
        /// </summary>
        internal SocketAsyncEventArgsPool(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool
        ///
        /// The "item" parameter is the SocketAsyncEventArgs instance 
        /// to add to the pool
        /// </summary>
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (_pool)
            {
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool
        /// and returns the object removed from the pool
        /// </summary>
        internal SocketAsyncEventArgs Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        ///<summary>The number of SocketAsyncEventArgs instances in the pool</summary>
        internal int Count
        {
            get { return _pool.Count; }
        }

        internal Int32 AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref _nextTokenId);
            return tokenId;
        }
    }
}
