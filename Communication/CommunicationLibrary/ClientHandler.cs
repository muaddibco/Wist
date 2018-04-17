using CommunicationLibrary.Interfaces;
using log4net;
using System;
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
        private readonly IMessagesHandler _messagesHandler;
        private readonly IBufferManager _bufferManager;
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;

        private CancellationTokenSource _cancellationTokenSource;

        public const byte STX = 0x02;
        public const byte DLE = 0x10;

        private int _offsetReceive;
        private int _offsetSend;
        private int _sendReceiveBufferSize;
        private bool _keepAlive;

        private bool _isBusy;
        private byte[] _currentBuf;
        private byte[] _currentPacket;

        private byte[] _tempLengthBuf = new byte[4]; // Packet size can include up to 4 bytes because if length bytes values are equal to either DLE or STX they'll be encoded with DLE
        private byte _tempLengthBufSize = 0;

        private bool _packetStartFound;
        private bool _lastPrevBufByteIsDle;
        private bool _lengthIsSet;
        private ushort _packetLengthExpected;
        private ushort _packetLengthRemained;

        public event EventHandler<EventArgs> SocketClosedEvent;

        public ClientHandler(IBufferManager bufferManager, IMessagesHandler messagesHandler)
        {
            _bufferManager = bufferManager;
            _messagesHandler = messagesHandler;
            _packets = new Queue<byte[]>();
            _messagePackets = new Queue<byte[]>();
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
        }

        public Queue<byte[]> MessagePackets => _messagePackets;

        public int TokenId { get; private set; }


        public IEnumerable<byte[]> GetMessagesToSend()
        {
            throw new NotImplementedException();
        }

        public void PushBuffer(byte[] buf, int count)
        {
            byte[] packet = new byte[count];
            Buffer.BlockCopy(buf, 0, packet, 0, count);
            _packets.Enqueue(packet);
            ParseReceivedData();
        }

        public void Start()
        {
            Stop();

            //_cancellationTokenSource = new CancellationTokenSource();
            //Task.Factory.StartNew(() => ParseReceivedData(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
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
                                            _messagesHandler.Push(_currentPacket);
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

            SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
            readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            //DataHoldingUserToken token = ServiceLocator.Current.GetInstance<DataHoldingUserToken>();
            //token.Init(tokenId, readWriteEventArg.Offset, readWriteEventArg.Offset + _settings.ReceiveBufferSize);
            //readWriteEventArg.UserToken = token;

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            _bufferManager.SetBuffer(readWriteEventArg);
            _offsetReceive = readWriteEventArg.Offset;
            _offsetSend = readWriteEventArg.Offset + sendReceiveBufferSize;
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            //DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)e.UserToken;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    _log.Info($"IO_Completed method in Receive, receiveSendToken id {TokenId}");
                    ProcessReceive();
                    break;

                case SocketAsyncOperation.Send:
                    _log.Info($"IO_Completed method in Send, id {TokenId}");

                    //ProcessSend(e);
                    break;

                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void StartReceive()
        {
            //DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            _log.Info($"Start receive for tokenId {TokenId}");

            _socketAsyncEventArgs.SetBuffer(_offsetReceive, _sendReceiveBufferSize);

            bool willRaiseEvent = _socketAsyncEventArgs.AcceptSocket.ReceiveAsync(_socketAsyncEventArgs);

            if (!willRaiseEvent)
            {
                _log.Info($"StartReceive in if (!willRaiseEvent), tokenId {TokenId}");

                ProcessReceive();
            }
        }

        private void ProcessReceive()
        {
            //DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            // If there was a socket error, close the connection. This is NOT a normal
            // situation, if you get an error here.
            // In the Microsoft example code they had this error situation handled
            // at the end of ProcessReceive. Putting it here improves readability
            // by reducing nesting some.
            if (_socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                _log.Error($"ProcessReceive ERROR, tokenId {TokenId}");

                //receiveSendToken.Reset();
                CloseClientSocket();

                return;
            }

            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (_socketAsyncEventArgs.BytesTransferred == 0)
            {
                _log.Info($"ProcessReceive NO DATA, receiveSendToken id {TokenId}");

                if (!_keepAlive)
                {
                    CloseClientSocket();
                }
                return;
            }

            //The BytesTransferred property tells us how many bytes 
            //we need to process.
            Int32 remainingBytesToProcess = _socketAsyncEventArgs.BytesTransferred;

            _log.Info($"ProcessReceive {TokenId}. remainingBytesToProcess = {remainingBytesToProcess}");

            PushBuffer(_socketAsyncEventArgs.Buffer, _socketAsyncEventArgs.BytesTransferred);

            StartReceive();
        }

        private void CloseClientSocket()
        {
            //var receiveSendToken = (e.UserToken as DataHoldingUserToken);

            _log.Info($"Closing client socket with tokenId {TokenId}");

            try
            {
                _log.Info($"Trying shutdown for tokenId {TokenId}");
                _socketAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _log.Warn($"Socket shutdown failed, tokenId {TokenId}", ex);
            }

            _socketAsyncEventArgs.AcceptSocket.Close();

            SocketClosedEvent?.Invoke(this, null);

        }

        public void AcceptSocket(Socket acceptSocket)
        {
            _log.Info($"Socket accepted by ClientHandler with tokenId {TokenId}.  Remote endpoint = {IPAddress.Parse(((IPEndPoint)acceptSocket.RemoteEndPoint).Address.ToString())}:{((IPEndPoint)acceptSocket.RemoteEndPoint).Port.ToString()}");

            _socketAsyncEventArgs.AcceptSocket = acceptSocket;

            StartReceive();
        }
    }
}
