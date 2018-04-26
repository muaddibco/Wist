using CommunicationLibrary.Interfaces;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    public class ClientHandler : IClientHandler
    {
        private readonly object _sync = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(ClientHandler));
        private readonly Queue<byte[]> _packets;
        private readonly Queue<byte[]> _messagePackets;
        private readonly ConcurrentQueue<byte[]> _postedMessages;
        private readonly IPacketsHandler _packetsHandler;
        private readonly IBufferManager _bufferManager;
        private readonly SocketAsyncEventArgs _socketReceiveAsyncEventArgs;
        private readonly SocketAsyncEventArgs _socketSendAsyncEventArgs;
        private readonly SocketAsyncEventArgs _socketConnectAsyncEventArgs;

        private CancellationTokenSource _cancellationTokenSource;

        public const byte STX = 0x02;
        public const byte DLE = 0x10;

        private int _offsetReceive;
        private int _offsetSend;
        private int _sendReceiveBufferSize;
        private bool _keepAlive;

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

        public event EventHandler<EventArgs> SocketClosedEvent;

        public ClientHandler(IBufferManager bufferManager, IPacketsHandler messagesHandler)
        {
            _bufferManager = bufferManager;
            _packetsHandler = messagesHandler;
            _packets = new Queue<byte[]>();
            _messagePackets = new Queue<byte[]>();
            _postedMessages = new ConcurrentQueue<byte[]>();
            _socketReceiveAsyncEventArgs = new SocketAsyncEventArgs();
            _socketReceiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            _socketSendAsyncEventArgs = new SocketAsyncEventArgs();
            _socketSendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
            _socketConnectAsyncEventArgs = new SocketAsyncEventArgs();
            _socketConnectAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
        }

        public Queue<byte[]> MessagePackets => _messagePackets;

        public int TokenId { get; private set; }


        public IEnumerable<byte[]> GetMessagesToSend()
        {
            throw new NotImplementedException();
        }

        public void PushBuffer(byte[] buf, int count)
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
                _log.Error($"Failure during pushing to buffer of ClientHandler with TokenId {TokenId}", ex);
            }
        }

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

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public void Init(int tokenId, int sendReceiveBufferSize, bool keepAlive)
        {
            TokenId = tokenId;
            _sendReceiveBufferSize = sendReceiveBufferSize;
            _keepAlive = keepAlive;

            _bufferManager.SetBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);
            _offsetReceive = _socketReceiveAsyncEventArgs.Offset;
            _offsetSend = _socketSendAsyncEventArgs.Offset;
        }

        public void AcceptSocket(Socket acceptSocket)
        {
            _log.Info($"Socket accepted by ClientHandler with tokenId {TokenId}.  Remote endpoint = {IPAddress.Parse(((IPEndPoint)acceptSocket.RemoteEndPoint).Address.ToString())}:{((IPEndPoint)acceptSocket.RemoteEndPoint).Port.ToString()}");

            _socketReceiveAsyncEventArgs.AcceptSocket = acceptSocket;
            _socketSendAsyncEventArgs.AcceptSocket = acceptSocket;

            StartReceive();
        }

        public void PostMessage(byte[] message)
        {
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

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                _log.Info($"Connect_Completed method in Connect, TokenId is {TokenId}");
                AcceptSocket(e.ConnectSocket);
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a connect.");
            }
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                _log.Info($"IO_Completed method in Receive, TokenId is {TokenId}");
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
                _log.Info($"Send_Completed method in Send, TokenId is {TokenId}");

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

            _log.Info($"Start receive for tokenId {TokenId}");

            try
            {
                _socketReceiveAsyncEventArgs.SetBuffer(_offsetReceive, _sendReceiveBufferSize);

                bool willRaiseEvent = _socketReceiveAsyncEventArgs.AcceptSocket.ReceiveAsync(_socketReceiveAsyncEventArgs);

                if (!willRaiseEvent)
                {
                    _log.Info($"StartReceive in if (!willRaiseEvent), tokenId {TokenId}");

                    ProcessReceive();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during StartReceive in ClientHandler with tokenId {TokenId}", ex);
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
                _log.Error($"ProcessReceive ERROR, tokenId {TokenId}");

                CloseClientSocket();

                return;
            }

            Int32 remainingBytesToProcess = _socketReceiveAsyncEventArgs.BytesTransferred;

            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (remainingBytesToProcess == 0)
            {
                _isReceiving = false;

                _log.Info($"ProcessReceive NO DATA, TokenId is {TokenId}");

                if (!_keepAlive)
                {
                    CloseClientSocket();
                }

                return;
            }

            _log.Info($"ProcessReceive {TokenId}. remainingBytesToProcess = {remainingBytesToProcess}");

            PushBuffer(_socketReceiveAsyncEventArgs.Buffer, _socketReceiveAsyncEventArgs.BytesTransferred);

            StartReceive();
        }

        private void CloseClientSocket()
        {
            _log.Info($"Closing client socket with tokenId {TokenId}");

            try
            {
                _log.Info($"Trying shutdown for tokenId {TokenId}");
                _socketReceiveAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _log.Warn($"Socket shutdown failed, tokenId {TokenId}", ex);
            }

            _socketReceiveAsyncEventArgs.AcceptSocket.Close();

            _bufferManager.FreeBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);

            SocketClosedEvent?.Invoke(this, null);
        }

        private void StartSend()
        {
            try
            {
                if (_postMessageRemainedBytes == 0)
                {
                    lock (_postedMessages)
                    {
                        if (_postedMessages.TryDequeue(out _currentPostMessage))
                        {
                            _postMessageRemainedBytes = _currentPostMessage.Length;
                        }
                        else
                        {
                            _isSending = false;
                            return;
                        }
                    }
                }

                if (_postMessageRemainedBytes <= _sendReceiveBufferSize)
                {
                    _socketReceiveAsyncEventArgs.SetBuffer(_offsetSend, _postMessageRemainedBytes);
                    Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketReceiveAsyncEventArgs.Buffer, _offsetSend, _postMessageRemainedBytes);
                }
                else
                {
                    _socketReceiveAsyncEventArgs.SetBuffer(_offsetSend, _sendReceiveBufferSize);
                    Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketReceiveAsyncEventArgs.Buffer, _offsetSend, _sendReceiveBufferSize);
                }

                bool willRaiseEvent = _socketReceiveAsyncEventArgs.AcceptSocket.SendAsync(_socketReceiveAsyncEventArgs);

                if (!willRaiseEvent)
                {
                    ProcessSend();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during StartSend at ClientHandler with TokenId {TokenId}", ex);
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
                _postMessageRemainedBytes -= _socketReceiveAsyncEventArgs.BytesTransferred;

                if (_socketReceiveAsyncEventArgs.SocketError == SocketError.Success)
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
                _log.Error($"Failure during ProcessSend at ClientHandler with TokenId {TokenId}", ex);
                CloseClientSocket();
            }
        }

        public void Connect(EndPoint endPoint)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socketConnectAsyncEventArgs.RemoteEndPoint = endPoint;
            bool willRaiseEvent = socket.ConnectAsync(_socketConnectAsyncEventArgs);
            if(!willRaiseEvent)
            {
                AcceptSocket(socket);
            }
        }
    }
}
