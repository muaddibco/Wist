using Wist.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Communication
{
    /// <summary>
    /// This class creates a single large buffer which can be divided up 
    /// and assigned to SocketAsyncEventArgs objects for use with each 
    /// socket I/O operation.  
    /// This enables buffers to be easily reused and guards against 
    /// fragmenting heap memory.
    /// 
    /// The operations exposed on the BufferManager class are not thread safe.
    /// </summary>
    [RegisterDefaultImplementation(typeof(IBufferManager), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class BufferManager : IBufferManager
    {
        private readonly Stack<int> _freeIndexPool;     
        private int _numBytes;                 // the total number of bytes controlled by the buffer pool
        private byte[] _buffer;                // the underlying byte array maintained by the Buffer Manager
        private int _currentIndex;
        private int _bufferSize;

        public BufferManager()
        {
            _currentIndex = 0;
            _freeIndexPool = new Stack<int>();
        }

        public int BufferSize => _bufferSize;

        /// <summary>
        /// Allocates buffer space used by the buffer pool 
        /// </summary>
        public void InitBuffer(int totalBytes, int bufferSize)
        {
            _numBytes = totalBytes;
            _bufferSize = bufferSize;
            // create one big large buffer and divide that 
            // out to each SocketAsyncEventArg object
            _buffer = new byte[_numBytes];
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the 
        /// specified SocketAsyncEventArgs object
        ///</summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public bool SetBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            if (receiveArgs == null)
            {
                throw new ArgumentNullException(nameof(receiveArgs));
            }

            if (sendArgs == null)
            {
                throw new ArgumentNullException(nameof(sendArgs));
            }

            if (_freeIndexPool.Count > 0)
            {
                int offset = _freeIndexPool.Pop();
                receiveArgs.SetBuffer(_buffer, offset, _bufferSize);
                sendArgs.SetBuffer(_buffer, offset + _bufferSize, _bufferSize);
            }
            else
            {
                if ((_numBytes - _bufferSize) < _currentIndex)
                {
                    return false;
                }
                receiveArgs.SetBuffer(_buffer, _currentIndex, _bufferSize);
                sendArgs.SetBuffer(_buffer, _currentIndex + _bufferSize, _bufferSize);
                _currentIndex += 2 * _bufferSize;
            }
            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  
        /// This frees the buffer back to the buffer pool
        /// </summary>
        public void FreeBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            _freeIndexPool.Push(receiveArgs.Offset);
            receiveArgs.SetBuffer(null, 0, 0);
            sendArgs.SetBuffer(null, 0, 0);
        }
    }
}
