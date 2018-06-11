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

namespace Wist.Communication
{
    public class CommunicationChannel : ICommunicationChannel
    {
        private readonly object _sync = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(CommunicationChannel));
        private readonly Queue<byte[]> _packets;
        private readonly Queue<byte[]> _messagePackets;
        private readonly ConcurrentQueue<IMessage> _postedMessages;
        private IBufferManager _bufferManager;
        private readonly SocketAsyncEventArgs _socketReceiveAsyncEventArgs;
        private readonly SocketAsyncEventArgs _socketSendAsyncEventArgs;
        private readonly ManualResetEventSlim _socketAcceptedEvent;

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

        private byte[] _tempLengthBuf = new byte[4]; // Packet size can include up to 4 bytes because if length bytes values are equal to either DLE or STX they'll be encoded with DLE
        private byte _tempLengthBufSize = 0;

        private bool _packetStartFound;
        private bool _lastPrevBufByteIsDle;
        private bool _lengthIsSet;
        private ushort _packetLengthExpected;
        private ushort _packetLengthRemained;

        private bool _isReceiving;
        private bool _isSending;
        private bool _isBusy;

        private Action<ICommunicationChannel, int> _onReceivedAction;

        public event EventHandler<EventArgs> SocketClosedEvent;

        public CommunicationChannel()
        {
            _packets = new Queue<byte[]>();
            _messagePackets = new Queue<byte[]>();
            _postedMessages = new ConcurrentQueue<IMessage>();
            _socketReceiveAsyncEventArgs = new SocketAsyncEventArgs();
            _socketReceiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            _socketSendAsyncEventArgs = new SocketAsyncEventArgs();
            _socketSendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
            _socketAcceptedEvent = new ManualResetEventSlim(false);
        }

        #region ICommunicationChannel implementation

        public Queue<byte[]> MessagePackets => _messagePackets;

        public IPAddress RemoteIPAddress { get; set; }

        public void PushForParsing(byte[] buf, int count)
        {
            try
            {
                byte[] packet = new byte[count];
                Buffer.BlockCopy(buf, 0, packet, 0, count);
                _packets.Enqueue(packet);
                ParseReceivedData();
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during pushing to buffer of Communication Channel with IP {RemoteIPAddress}", ex);
            }
        }

        public void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler, Action<ICommunicationChannel, int> onReceivedAction = null)
        {
            _bufferManager = bufferManager;
            _packetsHandler = packetsHandler;

            _bufferManager.SetBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);
            _offsetReceive = _socketReceiveAsyncEventArgs.Offset;
            _offsetSend = _socketSendAsyncEventArgs.Offset;
            _onReceivedAction = onReceivedAction;
        }

        public void AcceptSocket(Socket acceptSocket)
        {
            _log.Info($"Socket accepted by Communication channel.  Remote endpoint = {IPAddress.Parse(((IPEndPoint)acceptSocket.RemoteEndPoint).Address.ToString())}:{((IPEndPoint)acceptSocket.RemoteEndPoint).Port.ToString()}");

            _socketAcceptedEvent.Set();

            RemoteIPAddress = ((IPEndPoint)acceptSocket.RemoteEndPoint).Address;

            _socketReceiveAsyncEventArgs.AcceptSocket = acceptSocket;
            _socketSendAsyncEventArgs.AcceptSocket = acceptSocket;

            StartReceive();
        }

        public void PostMessage(IMessage message)
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
            _isReceiving = true;

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
            finally
            {
                _isReceiving = false;
            }
        }

        private void ProcessReceive()
        {
            if (_socketReceiveAsyncEventArgs.SocketError != SocketError.Success)
            {
                _isReceiving = false;
                _log.Error($"ProcessReceive ended with SocketError={_socketReceiveAsyncEventArgs.SocketError} from IP {RemoteIPAddress}");

                CloseClientSocket();

                return;
            }

            Int32 remainingBytesToProcess = _socketReceiveAsyncEventArgs.BytesTransferred;

            if(remainingBytesToProcess > 0)
            {
                
            }

            _onReceivedAction?.Invoke(this, remainingBytesToProcess);

            _log.Info($"ProcessReceive from IP {RemoteIPAddress}. remainingBytesToProcess = {remainingBytesToProcess}");

            PushForParsing(_socketReceiveAsyncEventArgs.Buffer, _socketReceiveAsyncEventArgs.BytesTransferred);

            StartReceive();
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
                _log.Warn($"Socket shutdown failed for IP {RemoteIPAddress}", ex);
            }

            _socketReceiveAsyncEventArgs.AcceptSocket.Close();
            
            _bufferManager.FreeBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);

            SocketClosedEvent?.Invoke(this, null);
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
                        IMessage msg;
                        if (_postedMessages.TryDequeue(out msg))
                        {
                            try
                            {
                                _currentPostMessage = msg.GetBytes();

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

                if (_postMessageRemainedBytes <= _bufferManager.BufferSize)
                {
                    _socketSendAsyncEventArgs.SetBuffer(_offsetSend, _postMessageRemainedBytes);
                    Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketSendAsyncEventArgs.Buffer, _offsetSend, _postMessageRemainedBytes);
                }
                else
                {
                    _socketSendAsyncEventArgs.SetBuffer(_offsetSend, _bufferManager.BufferSize);
                    Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketSendAsyncEventArgs.Buffer, _offsetSend, _bufferManager.BufferSize);
                }

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


        private async Task ParseReceivedData()
        {
            if (_isBusy)
                return;

            await Task.Run(() =>
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
                                        Buffer.BlockCopy(_currentBuf, offset, _currentPacket, _packetLengthExpected - _packetLengthRemained, bytesToCopy);
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
            });

            if (_packets.Count > 0)
            {
                await ParseReceivedData();
            }
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

        private int SetPacketLength(int offset)
        {
            byte b1 = 0, b2 = 0;
            do
            {
                _tempLengthBuf[_tempLengthBufSize++] = _currentBuf[offset++];

                if (_tempLengthBufSize > 1)
                {
                    b1 = (byte)(_tempLengthBuf[0] == DLE ? _tempLengthBuf[1] - DLE : _tempLengthBuf[0]);

                    if (_tempLengthBuf[0] == DLE)
                    {
                        if (_tempLengthBufSize > 2)
                        {
                            if (_tempLengthBuf[2] == DLE)
                            {
                                if (_tempLengthBufSize > 3)
                                {
                                    b2 = (byte)(_tempLengthBuf[3] - DLE);
                                    _lengthIsSet = true;
                                }
                            }
                            else
                            {
                                b2 = _tempLengthBuf[2];
                                _lengthIsSet = true;
                            }
                        }
                    }
                    else
                    {
                        b2 = _tempLengthBuf[1];
                        _lengthIsSet = true;
                    }
                }
            } while (_currentBuf.Length >= offset && !_lengthIsSet);

            if (_lengthIsSet)
            {
                _packetLengthExpected = (ushort)(b1 + (b2 << 8));
                _packetLengthRemained = _packetLengthExpected;
            }

            return offset;
        }

        #endregion Parsing functionality
    }
}
