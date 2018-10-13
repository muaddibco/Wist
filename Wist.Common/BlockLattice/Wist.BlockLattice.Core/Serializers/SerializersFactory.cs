using CommonServiceLocator;
using System;
using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Serializers
{
    [RegisterDefaultImplementation(typeof(ISerializersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class SerializersFactory : ISerializersFactory
    {
        private readonly Dictionary<PacketType, Dictionary<ushort, Stack<ISerializer>>> _serializersCache;
        private readonly object _sync = new object();

        public SerializersFactory(ISerializer[] signatureSupportSerializers)
        {
            _serializersCache = new Dictionary<PacketType, Dictionary<ushort, Stack<ISerializer>>>();

            foreach (var signatureSupportSerializer in signatureSupportSerializers)
            {
                if(!_serializersCache.ContainsKey(signatureSupportSerializer.PacketType))
                {
                    _serializersCache.Add(signatureSupportSerializer.PacketType, new Dictionary<ushort, Stack<ISerializer>>());
                }

                if(!_serializersCache[signatureSupportSerializer.PacketType].ContainsKey(signatureSupportSerializer.BlockType))
                {
                    _serializersCache[signatureSupportSerializer.PacketType].Add(signatureSupportSerializer.BlockType, new Stack<ISerializer>());
                }

                _serializersCache[signatureSupportSerializer.PacketType][signatureSupportSerializer.BlockType].Push(signatureSupportSerializer);
            }
        }

        private ISerializer Create(PacketType packetType, ushort blockType)
        {
            if(!_serializersCache.ContainsKey(packetType))
            {
                throw new PacketTypeNotSupportedBySignatureSupportingSerializersException(packetType);
            }

            if(!_serializersCache[packetType].ContainsKey(blockType))
            {
                throw new BlockTypeNotSupportedBySignatureSupportingSerializersException(packetType, blockType);
            }

            lock(_sync)
            {
                ISerializer serializer = null;

                if(_serializersCache[packetType][blockType].Count > 1)
                {
                    serializer = _serializersCache[packetType][blockType].Pop();
                }
                else
                {
                    ISerializer template = _serializersCache[packetType][blockType].Pop();
                    serializer = (ISerializer)ServiceLocator.Current.GetInstance(template.GetType());
                    _serializersCache[packetType][blockType].Push(template);
                }

                return serializer;
            }
        }

        public ISerializer Create(BlockBase block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            ISerializer serializer = Create(block.PacketType, block.BlockType);
            serializer.Initialize(block);

            return serializer;
        }

        public void Utilize(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (!_serializersCache.ContainsKey(serializer.PacketType))
            {
                throw new PacketTypeNotSupportedBySignatureSupportingSerializersException(serializer.PacketType);
            }

            if (!_serializersCache[serializer.PacketType].ContainsKey(serializer.BlockType))
            {
                throw new BlockTypeNotSupportedBySignatureSupportingSerializersException(serializer.PacketType, serializer.BlockType);
            }

            _serializersCache[serializer.PacketType][serializer.BlockType].Push(serializer);
        }
    }
}
