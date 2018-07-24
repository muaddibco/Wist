using Wist.Communication.Interfaces;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Communication;
using Wist.Core.Logging;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using System.IO;
using Wist.Communication.Exceptions;
using Wist.Core.ExtensionMethods;
using System.Buffers.Binary;
using System.Collections;

namespace Wist.Communication
{
    [RegisterDefaultImplementation(typeof(ICommunicationChannel), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class CommunicationChannel : ICommunicationChannel
    {
        private readonly object _sync = new object();
        private readonly ILogger _log;
        private readonly Queue<byte[]> _packets;
        private readonly Queue<byte[]> _messagePackets;
        private readonly ConcurrentQueue<byte[]> _postedMessages;
        private IBufferManager _bufferManager;
        private readonly SocketAsyncEventArgs _socketReceiveAsyncEventArgs;
        private readonly SocketAsyncEventArgs _socketSendAsyncEventArgs;
        private readonly ManualResetEventSlim _socketAcceptedEvent;
        private readonly MemoryStream _memoryStream;
        private readonly BinaryWriter _binaryWriter;
        
        private IPacketsHandler _packetsHandler;
        private CancellationTokenSource _cancellationTokenSource;

        public const byte STX = 0x02;
        public const byte DLE = 0x10;

        private int _offsetReceive;
        private int _offsetSend;

        private byte[] _currentBuf;
        private byte[] _currentPacket;
        private byte[] _currentPostMessage;
        private int _postMessageRemainedBytes;

        private byte[] _tempLengthBuf = new byte[8]; // Packet size can include up to 8 bytes because if length bytes values are equal to either DLE or STX they'll be encoded with DLE
        private byte _tempLengthBufSize = 0;

        private bool _packetStartFound;
        private bool _lastPrevBufByteIsDle;
        private bool _lengthIsSet;
        private uint _packetLengthExpected;
        private uint _packetLengthRemained;

        private bool _isSending;
        private bool _isBusy;
        private bool _disposed = false; // To detect redundant calls

        private Func<ICommunicationChannel, IPEndPoint, int, bool> _onReceivedExtendedValidation;

        public event EventHandler<EventArgs> SocketClosedEvent;

        public CommunicationChannel(ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packets = new Queue<byte[]>();
            _messagePackets = new Queue<byte[]>();
            _postedMessages = new ConcurrentQueue<byte[]>();
            _socketReceiveAsyncEventArgs = new SocketAsyncEventArgs();
            _socketReceiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            _socketSendAsyncEventArgs = new SocketAsyncEventArgs();
            _socketSendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
            _socketAcceptedEvent = new ManualResetEventSlim(false);

            _memoryStream = new MemoryStream();
            _binaryWriter = new BinaryWriter(_memoryStream);
        }

        #region ICommunicationChannel implementation

        public Queue<byte[]> MessagePackets => _messagePackets;

        public IPAddress RemoteIPAddress { get; set; } = IPAddress.None;

        public void PushForParsing(byte[] buf, int offset, int count)
        {
            try
            {
                byte[] packet = new byte[count];
                Buffer.BlockCopy(buf, offset, packet, 0, count);

                _log.Debug(packet.ToHexString());

                _packets.Enqueue(packet);

                ParseReceivedData();
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during pushing to buffer of Communication Channel with IP {RemoteIPAddress}", ex);
            }
        }

        public void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler)
        {
            _bufferManager = bufferManager;
            _packetsHandler = packetsHandler;

            _bufferManager.SetBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);
            _offsetReceive = _socketReceiveAsyncEventArgs.Offset;
            _offsetSend = _socketSendAsyncEventArgs.Offset;
        }

        public void RegisterExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceivedExtendedValidation)
        {
            _onReceivedExtendedValidation = onReceivedExtendedValidation;
        }

        public void AcceptSocket(Socket acceptSocket)
        {
            _log.Info($"Socket accepted by Communication channel.  Remote endpoint = {IPAddress.Parse(((IPEndPoint)acceptSocket.LocalEndPoint).Address.ToString())}:{((IPEndPoint)acceptSocket.LocalEndPoint).Port.ToString()}");

            if (acceptSocket.Connected)
            {
                RemoteIPAddress = ((IPEndPoint)acceptSocket.RemoteEndPoint).Address;
            }

            _socketReceiveAsyncEventArgs.AcceptSocket = acceptSocket;
            _socketSendAsyncEventArgs.AcceptSocket = acceptSocket;

            _socketAcceptedEvent.Set();

            StartReceive();
        }

        public void PostMessage(byte[] message)
        {
            //TODO: weigh refactoring so BlockingCollection will be used
            lock (_postedMessages)
            {
                _postedMessages.Enqueue(message);

                if (!_isSending)
                {
                    _isSending = true;

                    Task.Factory.StartNew(() => StartSend(), TaskCreationOptions.LongRunning);
                }
            }
        }

        public void Close()
        {
            CloseClientSocket();
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        #endregion ICommunicationChannel implementation

        #region Private functions

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                _log.Info($"Receive_Completed from IP {RemoteIPAddress}");
                ProcessReceive();
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive.");
            }
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                _log.Info($"Send_Completed to IP {RemoteIPAddress}");

                ProcessSend();
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a send");
            }
        }

        private void StartReceive()
        {
            _log.Info($"Start receive from IP {RemoteIPAddress}");

            try
            {
                bool willRaiseEvent = _socketReceiveAsyncEventArgs.AcceptSocket.ReceiveAsync(_socketReceiveAsyncEventArgs);

                if (!willRaiseEvent)
                {
                    _log.Info($"Going to ProcessReceive from IP {RemoteIPAddress}");

                    ProcessReceive();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during StartReceive from IP {RemoteIPAddress}", ex);
                throw;
            }
        }

        private void ProcessReceive()
        {
            if (_socketReceiveAsyncEventArgs.SocketError != SocketError.Success)
            {
                _log.Error($"ProcessReceive ended with SocketError={_socketReceiveAsyncEventArgs.SocketError} from IP {RemoteIPAddress}");

                CloseClientSocket();

                return;
            }
            
            Int32 remainingBytesToProcess = _socketReceiveAsyncEventArgs.BytesTransferred;

            if (_onReceivedExtendedValidation?.Invoke(this, _socketReceiveAsyncEventArgs.RemoteEndPoint as IPEndPoint, remainingBytesToProcess) ?? true)
            {
                if (remainingBytesToProcess > 0)
                {
                    _log.Info($"ProcessReceive from IP {RemoteIPAddress}. remainingBytesToProcess = {remainingBytesToProcess}");

                    PushForParsing(_socketReceiveAsyncEventArgs.Buffer, _socketReceiveAsyncEventArgs.Offset, _socketReceiveAsyncEventArgs.BytesTransferred);
                }

                StartReceive();
            }
        }

        private void CloseClientSocket()
        {
            _log.Info($"Closing client socket with IP {RemoteIPAddress}");

            try
            {
                _log.Info($"Trying shutdown Socket with IP {RemoteIPAddress}");
                _socketReceiveAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _log.Warning($"Socket shutdown failed for IP {RemoteIPAddress}", ex);
            }

            _socketReceiveAsyncEventArgs.AcceptSocket.Close();
            
            _bufferManager.FreeBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);

            SocketClosedEvent?.Invoke(this, null);
        }

        private void WriteByteWithEncoding(BinaryWriter bw, byte b)
        {
            if (b == DLE || b == STX)
            {
                bw.Write(DLE);
                b += DLE;
            }

            bw.Write(b);
        }

        private byte[] GetEscapedPacketBytes(byte[] packet)
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            _binaryWriter.Write(DLE);
            _binaryWriter.Write(STX);

            uint packetLength = (uint)packet.Length;

            Span<byte> span = new Span<byte>();
            Memory<byte> memory = new Memory<byte>();
            ArraySegment<byte> vs = new ArraySegment<byte>();
            ArrayList arrayList = new ArrayList();

            long pos1 = _memoryStream.Position;
            _memoryStream.Seek(8, SeekOrigin.Current);

            for (int i = 0; i < packet.Length; i++)
            {
                WriteByteWithEncoding(_binaryWriter, packet[i]);
            }

            long pos2 = _memoryStream.Position;
            _memoryStream.Seek(pos1, SeekOrigin.Begin);
            uint length = (uint)(pos2 - pos1 - 8);
            byte[] lengthByte = BitConverter.GetBytes(length);
            WriteByteWithEncoding(_binaryWriter, lengthByte[0]);
            WriteByteWithEncoding(_binaryWriter, lengthByte[1]);
            WriteByteWithEncoding(_binaryWriter, lengthByte[2]);
            WriteByteWithEncoding(_binaryWriter, lengthByte[3]);
            _memoryStream.Seek(pos2, SeekOrigin.Begin);

            return _memoryStream.ToArray();
        }

        private void StartSend()
        {
            _socketAcceptedEvent.Wait();

            try
            {
                if (_postMessageRemainedBytes == 0)
                {
                    lock (_postedMessages)
                    {
                        byte[] msg;
                        if (_postedMessages.TryDequeue(out msg))
                        {
                            try
                            {
                                _currentPostMessage = GetEscapedPacketBytes(msg);

                                _postMessageRemainedBytes = _currentPostMessage.Length;
                            }
                            catch (Exception)
                            {
                                StartSend();
                            }
                        }
                        else
                        {
                            _isSending = false;
                            return;
                        }
                    }
                }

                int length = _bufferManager.BufferSize;
                if (_postMessageRemainedBytes <= _bufferManager.BufferSize)
                {
                    length = _postMessageRemainedBytes;
                }

                _socketSendAsyncEventArgs.SetBuffer(_offsetSend, length);
                Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketSendAsyncEventArgs.Buffer, _offsetSend, length);

                _log.Debug($"Sending bytes: {_socketSendAsyncEventArgs.Buffer.ToHexString(_socketSendAsyncEventArgs.Offset, length)}");

                bool willRaiseEvent = _socketSendAsyncEventArgs.AcceptSocket.SendAsync(_socketSendAsyncEventArgs);

                if (!willRaiseEvent)
                {
                    ProcessSend();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during StartSend to IP {RemoteIPAddress}", ex);
                CloseClientSocket();
            }
            finally
            {
                _isSending = false;
            }
        }

        private void ProcessSend()
        {
            try
            {
                _postMessageRemainedBytes -= _socketSendAsyncEventArgs.BytesTransferred;

                if (_socketSendAsyncEventArgs.SocketError == SocketError.Success)
                {
                    StartSend();
                }
                else
                {
                    _isSending = false;
                    CloseClientSocket();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during ProcessSend to IP {RemoteIPAddress}", ex);
                CloseClientSocket();
            }
        }

        #endregion Private functions

        #region Parsing functionality


        private void ParseReceivedData()
        {
            if (_isBusy)
                return;

            Task.Run(() =>
            {
                lock (_sync)
                {
                    if (_isBusy)
                        return;

                    _isBusy = true;
                }

                try
                {
                    while (_packets.Count > 0)
                    {
                        _currentBuf = _packets.Dequeue();

                        _log.Debug($"Picked bytes for parsing: {_currentBuf.ToHexString()}");

                        int offset = 0;

                        do
                        {
                            if (!_packetStartFound)
                            {
                                if (_lastPrevBufByteIsDle && _currentBuf[0] == STX)
                                {
                                    _packetStartFound = true;
                                    offset++;
                                }
                                else
                                {
                                    for (; offset < _currentBuf.Length - 1; offset++)
                                    {
                                        if (_currentBuf[offset] == DLE && _currentBuf[offset + 1] == STX)
                                        {
                                            _packetStartFound = true;
                                            offset += 2;
                                            break;
                                        }
                                    }

                                    if (!_packetStartFound)
                                    {
                                        offset++;
                                        _lastPrevBufByteIsDle = _currentBuf[_currentBuf.Length - 1] == DLE;
                                    }
                                }
                            }

                            if (_packetStartFound)
                            {
                                if (!_lengthIsSet)
                                {
                                    offset = SetPacketLength(offset);
                                }

                                if (_lengthIsSet)
                                {
                                    if (_packetLengthRemained == _packetLengthExpected)
                                    {
                                        _currentPacket = new byte[_packetLengthExpected];
                                    }

                                    if (_currentBuf.Length > offset)
                                    {
                                        ushort bytesToCopy = (ushort)Math.Min(_currentBuf.Length - offset, _packetLengthRemained);
                                        //TODO: seems will be bug with huge packets!!!
                                        Buffer.BlockCopy(_currentBuf, offset, _currentPacket, (int)(_packetLengthExpected - _packetLengthRemained), bytesToCopy);
                                        _packetLengthRemained -= bytesToCopy;
                                        offset += bytesToCopy;

                                        if (_packetLengthRemained == 0)
                                        {
                                            _packetsHandler.Push(_currentPacket);
                                            Reset();
                                        }
                                    }
                                }
                            }
                        } while (_currentBuf.Length > offset);
                    }

                    _isBusy = false;
                }
                catch (Exception ex)
                {
                    _isBusy = false;
                    _log.Error("Failed to parse packet", ex);
                }
                finally
                {
                    if (_packets.Count > 0)
                    {
                        ParseReceivedData();
                    }
                }
            });
        }

        private void Reset()
        {
            _packetStartFound = false;
            _lengthIsSet = false;
            _lastPrevBufByteIsDle = false;
            _packetLengthExpected = 0;
            _packetLengthRemained = 0;
            _tempLengthBufSize = 0;
        }

        private bool TryFetchLength(byte[] buffer, int bufLen, out uint length)
        {
            byte[] lenBytes = new byte[4];
            byte lenFilled = 0;
            bool dle = false;

            for (int i = 0; i < bufLen && lenFilled < 4; i++)
            {
                if(buffer[i] == DLE)
                {
                    dle = true;
                }
                else
                {
                    byte v = buffer[i];
                    if(dle)
                    {
                        dle = false;
                        v -= DLE;
                    }

                    lenBytes[lenFilled++] = v;
                }
            }

            if(lenFilled == 4)
            {
                length = ((uint)lenBytes[3] << 24) + ((uint)lenBytes[2] << 16) + ((uint)lenBytes[1] << 8) + (uint)lenBytes[0];
                return true;
            }

            length = 0;
            return false;
        }

        private int SetPacketLength(int offset)
        {
            do
            {
                _tempLengthBuf[_tempLengthBufSize++] = _currentBuf[offset++];

            } while (_currentBuf.Length >= offset && _tempLengthBufSize < 8);

            if (_tempLengthBufSize == 8)
            {
                _lengthIsSet = TryFetchLength(_tempLengthBuf, _tempLengthBufSize, out _packetLengthExpected);

                if (_lengthIsSet)
                {
                    _packetLengthRemained = _packetLengthExpected;
                }
            }

            return offset;
        }

        #endregion Parsing functionality

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _binaryWriter?.Dispose();
                    _memoryStream?.Dispose();
                }

                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
