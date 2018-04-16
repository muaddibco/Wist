using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    public class ClientHandler : IClientHandler
    {
        private readonly Queue<byte[]> _packets;
        private readonly Queue<byte[]> _messagePackets;
        private CancellationTokenSource _cancellationTokenSource;

        public const byte STX = 0x02;
        public const byte DLE = 0x10;

        private byte[] _currentBuf;
        private byte[] _currentPacket;

        private byte[] _tempLengthBuf = new byte[4]; // Packet size can include up to 4 bytes because if length bytes values are equal to either DLE or STX they'll be encoded with DLE
        private byte _tempLengthBufSize = 0;

        private bool _packetStartFound;
        private bool _lastPrevBufByteIsDle;
        private bool _lengthIsSet;
        private ushort _packetLengthExpected;
        private ushort _packetLengthRemained;

        public ClientHandler()
        {
            _packets = new Queue<byte[]>();
            _messagePackets = new Queue<byte[]>();
        }

        public Queue<byte[]> MessagePackets => _messagePackets;

        public IEnumerable<byte[]> GetMessagesToSend()
        {
            throw new NotImplementedException();
        }

        public void PushBuffer(byte[] buf, int count)
        {
            byte[] packet = new byte[count];
            Buffer.BlockCopy(buf, 0, packet, 0, count);
            _packets.Enqueue(packet);
        }

        public void Start()
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => ParseReceivedData(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        private void ParseReceivedData(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                if(_packets.Count > 0)
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

                                if(!_packetStartFound)
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

                                    if(_packetLengthRemained == 0)
                                    {
                                        // TODO: take care about parsed message
                                        _messagePackets.Enqueue(_currentPacket);
                                        Reset();
                                    }
                                }
                            }
                        }
                    } while (_currentBuf.Length > offset);
                }
                else
                {
                    Thread.Sleep(50);
                }
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
    }
}
