using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Wist.Core
{
    public class GenericPool<T> : IEnumerable<T> where T : class
    {
        Stack<T> _pool;

        private Int32 _nextTokenId = 0;

        /// <summary>
        /// Initializes the object pool to the specified size
        ///
        /// The "capacity" parameter is the maximum number of 
        /// <see cref="T"/> objects the pool can hold
        /// </summary>
        public GenericPool(int capacity)
        {
            _pool = new Stack<T>(capacity);
        }

        /// <summary>
        /// Add a <see cref="T"/> instance to the pool
        ///
        /// The "item" parameter is the <see cref="T"/> instance 
        /// to add to the pool
        /// </summary>
        public void Push(T item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a GenericPool cannot be null"); }
            lock (_pool)
            {
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a <see cref="T"/> instance from the pool
        /// and returns the object removed from the pool
        /// </summary>
        public T Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        ///<summary>The number of <see cref="T"/> instances in the pool</summary>
        public int Count
        {
            get { return _pool.Count; }
        }

        public Int32 AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref _nextTokenId);
            return tokenId;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _pool.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _pool.GetEnumerator();
        }
    }
}
