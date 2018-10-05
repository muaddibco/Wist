using Wist.Network.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.Logging;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using System.IO;
using Wist.Core.ExtensionMethods;
using Wist.Core.PerformanceCounters;
using Wist.Network.PerformanceCounters;
using Wist.BlockLattice.Core.Handlers;

namespace Wist.Network.Communication
{
    [RegisterDefaultImplementation(typeof(ICommunicationChannel), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class CommunicationChannel : ICommunicationChannel
    {
        private readonly object _sync = new object();
        private readonly ILogger _log;
        private readonly ConcurrentQueue<byte[]> _packets;
        private readonly ConcurrentQueue<byte[]> _postedMessages;
        private IBufferManager _bufferManager;
        private readonly SocketAsyncEventArgs _socketReceiveAsyncEventArgs;
        private readonly SocketAsyncEventArgs _socketSendAsyncEventArgs;
        private readonly ManualResetEventSlim _socketAcceptedEvent;
        private readonly ManualResetEventSlim _socketSendEvent;
        private readonly MemoryStream _memoryStream;
        private readonly BinaryWriter _binaryWriter;
        private readonly CommunicationCountersService _communicationCountersService;
        
        private IPacketsHandler _packetsHandler;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public const byte STX = 0x02;
        public const byte DLE = 0x10;

        private int _offsetReceive;
        private int _offsetSend;

        private byte[] _currentPostMessage;
        private int _postMessageRemainedBytes;


        private bool _isSendProcessing;
        private bool _isBusy;
        private bool _disposed = false; // To detect redundant calls

        private Func<ICommunicationChannel, IPEndPoint, int, bool> _onReceivedExtendedValidation;

        public event EventHandler<EventArgs> SocketClosedEvent;

        public CommunicationChannel(ILoggerService loggerService, IPerformanceCountersRepository performanceCountersRepository)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packets = new ConcurrentQueue<byte[]>();
            _postedMessages = new ConcurrentQueue<byte[]>();
            _socketReceiveAsyncEventArgs = new SocketAsyncEventArgs();
            _socketReceiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            _socketSendAsyncEventArgs = new SocketAsyncEventArgs();
            _socketSendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
            _socketAcceptedEvent = new ManualResetEventSlim(false);
            _socketSendEvent = new ManualResetEventSlim();

            _memoryStream = new MemoryStream();
            _binaryWriter = new BinaryWriter(_memoryStream);

            _communicationCountersService = performanceCountersRepository?.GetInstance<CommunicationCountersService>();
        }

        #region ICommunicationChannel implementation

        public IPAddress RemoteIPAddress { get; set; } = IPAddress.None;

        public void PushForParsing(byte[] buf, int offset, int count)
        {
            try
            {
                byte[] packet = new byte[count];
                Buffer.BlockCopy(buf, offset, packet, 0, count);

                _log.Debug(packet.ToHexString());

                if (packet != null)
                {
                    _communicationCountersService?.ParsingQueueSize.Increment();
                    _packets.Enqueue(packet);
                }
                else
                {

                }

                if (!_isBusy)
                {
                    ParseReceivedData();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during pushing to buffer of Communication Channel with IP {RemoteIPAddress}", ex);
            }
        }

        public void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler)
        {
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _bufferManager = bufferManager;
            _packetsHandler = packetsHandler;

            _bufferManager.SetBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);
            _offsetReceive = _socketReceiveAsyncEventArgs.Offset;
            _offsetSend = _socketSendAsyncEventArgs.Offset;
        }

        //TODO: need to ascertain that this is really needed, looks weird
        public void RegisterExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceivedExtendedValidation)
        {
            _onReceivedExtendedValidation = onReceivedExtendedValidation;
        }

        public void AcceptSocket(Socket acceptSocket)
        {
            _communicationCountersService?.CommunicationChannels.Increment();

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
                _log.Debug($"Enqueueing message for sending {message.ToHexString()}");

                _postedMessages.Enqueue(message);

                if (!_isSendProcessing)
                {
                    _isSendProcessing = true;

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
                _log.Debug($"Receive_Completed from IP {RemoteIPAddress}");
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
                _socketSendEvent.Set();
                _log.Debug($"Send_Completed to IP {RemoteIPAddress}");

                ProcessSend();
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a send");
            }
        }

        private void StartReceive()
        {
            _log.Debug($"Start receive from IP {RemoteIPAddress}");

            if(!_socketReceiveAsyncEventArgs.AcceptSocket.Connected)
            {

            }

            //if (_socketReceiveAsyncEventArgs.AcceptSocket.Connected)
            {
                try
                {
                    bool willRaiseEvent = _socketReceiveAsyncEventArgs.AcceptSocket.ReceiveAsync(_socketReceiveAsyncEventArgs);

                    if (!willRaiseEvent)
                    {
                        _log.Debug($"Going to ProcessReceive from IP {RemoteIPAddress}");

                        ProcessReceive();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failure during StartReceive from IP {RemoteIPAddress}", ex);
                    throw;
                }
            }
            //else
            //{
            //    SocketIsDisconnectedException socketIsDisconnectedException = new SocketIsDisconnectedException((IPEndPoint)_socketReceiveAsyncEventArgs.AcceptSocket.RemoteEndPoint);
            //    _log.Error("Unexpected socket disconnection", socketIsDisconnectedException);
            //    throw socketIsDisconnectedException;
            //}
        }

        private void ProcessReceive()
        {
            if (_socketReceiveAsyncEventArgs.SocketError != SocketError.Success)
            {
                _communicationCountersService?.CommunicationErrors.Increment();
                _log.Error($"ProcessReceive ended with SocketError={_socketReceiveAsyncEventArgs.SocketError} from IP {RemoteIPAddress}");

                CloseClientSocket();

                return;
            }
            
            Int32 remainingBytesToProcess = _socketReceiveAsyncEventArgs.BytesTransferred;
            _communicationCountersService?.BytesReceived.IncrementBy(remainingBytesToProcess);

            if (_onReceivedExtendedValidation?.Invoke(this, _socketReceiveAsyncEventArgs.RemoteEndPoint as IPEndPoint, remainingBytesToProcess) ?? true)
            {
                if (remainingBytesToProcess > 0)
                {
                    _log.Debug($"ProcessReceive from IP {RemoteIPAddress}. remainingBytesToProcess = {remainingBytesToProcess}");

                    PushForParsing(_socketReceiveAsyncEventArgs.Buffer, _socketReceiveAsyncEventArgs.Offset, _socketReceiveAsyncEventArgs.BytesTransferred);
                }

                StartReceive();
            }
        }

        private void CloseClientSocket()
        {
            _communicationCountersService?.CommunicationChannels.Decrement();
            _log.Debug($"Closing client socket with IP {RemoteIPAddress}");

            try
            {
                _log.Debug($"Trying shutdown Socket with IP {RemoteIPAddress}");
                _socketReceiveAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _log.Warning($"Socket shutdown failed for IP {RemoteIPAddress}", ex);
            }

            _isSendProcessing = false;
            _socketReceiveAsyncEventArgs.AcceptSocket.Close();

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

            if (_cancellationToken.IsCancellationRequested)
                return;

            try
            {
                if (_postMessageRemainedBytes == 0)
                {
                    lock (_postedMessages)
                    {
                        if (_postedMessages.TryDequeue(out byte[] msg))
                        {
                            try
                            {
                                _log.Debug($"Message being sent {msg.ToHexString()}");

                                _currentPostMessage = GetEscapedPacketBytes(msg);

                                _log.Debug($"Escaped message being sent {_currentPostMessage.ToHexString()}");

                                _postMessageRemainedBytes = _currentPostMessage.Length;
                            }
                            catch (Exception)
                            {
                                StartSend();
                            }
                        }
                        else
                        {
                            _isSendProcessing = false;
                            return;
                        }
                    }
                }

                int length = _bufferManager.BufferSize;
                if (_postMessageRemainedBytes <= _bufferManager.BufferSize)
                {
                    length = _postMessageRemainedBytes;
                }

                _communicationCountersService?.BytesSent.IncrementBy(length);

                _socketSendAsyncEventArgs.SetBuffer(_offsetSend, length);
                Buffer.BlockCopy(_currentPostMessage, _currentPostMessage.Length - _postMessageRemainedBytes, _socketSendAsyncEventArgs.Buffer, _offsetSend, length);

                _log.Debug($"Sending bytes: {_socketSendAsyncEventArgs.Buffer.ToHexString(_socketSendAsyncEventArgs.Offset, length)}");

                _socketSendEvent.Reset();
                bool willRaiseEvent = _socketSendAsyncEventArgs.AcceptSocket.SendAsync(_socketSendAsyncEventArgs);

                if (!willRaiseEvent)
                {
                    ProcessSend();
                }
                else
                {
                    _socketSendEvent.Wait(_cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failure during StartSend to IP {RemoteIPAddress}", ex);
                CloseClientSocket();
            }
        }

        private void ProcessSend()
        {
            _postMessageRemainedBytes -= _socketSendAsyncEventArgs.BytesTransferred;

            try
            {
                if (_socketSendAsyncEventArgs.SocketError == SocketError.Success)
                {
                    StartSend();
                }
                else
                {
                    _communicationCountersService?.CommunicationErrors.Increment();
                    _isSendProcessing = false;
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
            lock (_sync)
            {
                if (_isBusy)
                    return;

                _isBusy = true;
            }

            Task.Factory.StartNew(() =>
            {
                byte[] currentPacket = null;
                byte[] tempLengthBuf = new byte[8]; // Packet size can include up to 8 bytes because if length bytes values are equal to either DLE or STX they'll be encoded with DLE
                byte tempLengthBufSize = 0;
                bool lengthIsSet = false;
                bool packetStartFound = false;
                bool lastPrevBufByteIsDle = false;
                uint packetLengthExpected = 0, packetLengthRemained = 0;

                try
                {
                    while (_packets.Count > 0)
                    {
                        if (!_packets.TryDequeue(out byte[] currentBuf))
                            continue;

                        _communicationCountersService?.ParsingQueueSize.Decrement();

                        if (currentBuf == null)
                            continue;

                        _log.Debug($"Picked bytes for parsing: {currentBuf.ToHexString()}");

                        int offset = 0;

                        do
                        {
                            if (!packetStartFound)
                            {
                                packetStartFound = CheckPacketStart(ref lastPrevBufByteIsDle, currentBuf, ref offset);
                            }

                            if (packetStartFound)
                            {
                                if (!lengthIsSet)
                                {
                                    lengthIsSet = TryGetPacketLength(ref offset, currentBuf, out packetLengthExpected, out packetLengthRemained, tempLengthBuf, ref tempLengthBufSize);
                                }

                                if (lengthIsSet)
                                {
                                    if (currentPacket == null)
                                    {
                                        currentPacket = new byte[packetLengthExpected];
                                    }

                                    if (currentBuf.Length > offset)
                                    {
                                        ushort bytesToCopy = (ushort)Math.Min(currentBuf.Length - offset, packetLengthRemained);
                                        //TODO: seems will be bug with huge packets!!!
                                        Buffer.BlockCopy(currentBuf, offset, currentPacket, (int)(packetLengthExpected - packetLengthRemained), bytesToCopy);
                                        packetLengthRemained -= bytesToCopy;
                                        offset += bytesToCopy;

                                        if (packetLengthRemained == 0)
                                        {
                                            _packetsHandler.Push(currentPacket);
                                            currentPacket = null;
                                            lengthIsSet = false;
                                            packetStartFound = false;
                                            lastPrevBufByteIsDle = false;
                                            packetLengthExpected = 0;
                                            packetLengthRemained = 0;
                                            tempLengthBufSize = 0;
                                        }
                                    }
                                }
                            }
                        } while (currentBuf.Length > offset);
                    }

                }
                catch (Exception ex)
                {
                    _log.Error("Failed to parse packet", ex);
                }
                finally
                {
                    _isBusy = false;

                    if (_packets.Count > 0)
                    {
                        ParseReceivedData();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static bool CheckPacketStart(ref bool lastPrevBufByteIsDle, byte[] currentBuf, ref int offset)
        {
            bool packetStartFound = false;
            if (lastPrevBufByteIsDle && currentBuf[0] == STX)
            {
                packetStartFound = true;
                offset++;
            }
            else
            {
                for (; offset < currentBuf.Length - 1; offset++)
                {
                    if (currentBuf[offset] == DLE && currentBuf[offset + 1] == STX)
                    {
                        packetStartFound = true;
                        offset += 2;
                        break;
                    }
                }

                if (!packetStartFound)
                {
                    offset++;
                    lastPrevBufByteIsDle = currentBuf[currentBuf.Length - 1] == DLE;
                }
            }

            return packetStartFound;
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

        private bool TryGetPacketLength(ref int offset, byte[] currentBuf, out uint packetLengthExpected, out uint packetLengthRemained, byte[] tempLengthBuf, ref byte tempLengthBufSize)
        {
            packetLengthExpected = 0;
            packetLengthRemained = 0;

            bool lengthIsSet = false;

            do
            {
                if (offset < currentBuf.Length)
                {
                    tempLengthBuf[tempLengthBufSize++] = currentBuf[offset++];
                }

            } while (currentBuf.Length > offset && tempLengthBufSize < 8);

            if (tempLengthBufSize == 8)
            {
                lengthIsSet = TryFetchLength(tempLengthBuf, tempLengthBufSize, out packetLengthExpected);

                if (lengthIsSet)
                {
                    packetLengthRemained = packetLengthExpected;
                }
            }

            return lengthIsSet;
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
