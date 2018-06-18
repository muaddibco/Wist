using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Serializers
{
    [RegisterDefaultImplementation(typeof(ISignatureSupportSerializersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class SignatureSupportSerializersFactory : ISignatureSupportSerializersFactory
    {
        private readonly Dictionary<PacketType, Dictionary<ushort, Stack<ISignatureSupportSerializer>>> _serializersCache;
        private readonly object _sync = new object();

        public SignatureSupportSerializersFactory(ISignatureSupportSerializer[] signatureSupportSerializers)
        {
            _serializersCache = new Dictionary<PacketType, Dictionary<ushort, Stack<ISignatureSupportSerializer>>>();

            foreach (var signatureSupportSerializer in signatureSupportSerializers)
            {
                if(!_serializersCache.ContainsKey(signatureSupportSerializer.PacketType))
                {
                    _serializersCache.Add(signatureSupportSerializer.PacketType, new Dictionary<ushort, Stack<ISignatureSupportSerializer>>());
                }

                if(!_serializersCache[signatureSupportSerializer.PacketType].ContainsKey(signatureSupportSerializer.BlockType))
                {
                    _serializersCache[signatureSupportSerializer.PacketType].Add(signatureSupportSerializer.BlockType, new Stack<ISignatureSupportSerializer>());
                }

                _serializersCache[signatureSupportSerializer.PacketType][signatureSupportSerializer.BlockType].Push(signatureSupportSerializer);
            }
        }

        public ISignatureSupportSerializer Create(PacketType packetType, ushort blockType)
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
                ISignatureSupportSerializer serializer = null;

                if(_serializersCache[packetType][blockType].Count > 1)
                {
                    serializer = _serializersCache[packetType][blockType].Pop();
                }
                else
                {
                    ISignatureSupportSerializer template = _serializersCache[packetType][blockType].Pop();
                    serializer = (ISignatureSupportSerializer)ServiceLocator.Current.GetInstance(template.GetType());
                    _serializersCache[packetType][blockType].Push(template);
                }

                return serializer;
            }
        }

        public void Utilize(ISignatureSupportSerializer serializer)
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
