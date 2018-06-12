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
                if(!_serializersCache.ContainsKey(signatureSupportSerializer.ChainType))
                {
                    _serializersCache.Add(signatureSupportSerializer.ChainType, new Dictionary<ushort, Stack<ISignatureSupportSerializer>>());
                }

                if(!_serializersCache[signatureSupportSerializer.ChainType].ContainsKey(signatureSupportSerializer.BlockType))
                {
                    _serializersCache[signatureSupportSerializer.ChainType].Add(signatureSupportSerializer.BlockType, new Stack<ISignatureSupportSerializer>());
                }

                _serializersCache[signatureSupportSerializer.ChainType][signatureSupportSerializer.BlockType].Push(signatureSupportSerializer);
            }
        }

        public ISignatureSupportSerializer Create(PacketType chainType, ushort blockType)
        {
            if(!_serializersCache.ContainsKey(chainType))
            {
                throw new ChainTypeNotSupportedBySignatureSupportingSerializersException(chainType);
            }

            if(!_serializersCache[chainType].ContainsKey(blockType))
            {
                throw new BlockTypeNotSupportedBySignatureSupportingSerializersException(chainType, blockType);
            }

            lock(_sync)
            {
                ISignatureSupportSerializer serializer = null;

                if(_serializersCache[chainType][blockType].Count > 1)
                {
                    serializer = _serializersCache[chainType][blockType].Pop();
                }
                else
                {
                    ISignatureSupportSerializer template = _serializersCache[chainType][blockType].Pop();
                    serializer = (ISignatureSupportSerializer)ServiceLocator.Current.GetInstance(template.GetType());
                    _serializersCache[chainType][blockType].Push(template);
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

            if (!_serializersCache.ContainsKey(serializer.ChainType))
            {
                throw new ChainTypeNotSupportedBySignatureSupportingSerializersException(serializer.ChainType);
            }

            if (!_serializersCache[serializer.ChainType].ContainsKey(serializer.BlockType))
            {
                throw new BlockTypeNotSupportedBySignatureSupportingSerializersException(serializer.ChainType, serializer.BlockType);
            }

            _serializersCache[serializer.ChainType][serializer.BlockType].Push(serializer);
        }
    }
}
