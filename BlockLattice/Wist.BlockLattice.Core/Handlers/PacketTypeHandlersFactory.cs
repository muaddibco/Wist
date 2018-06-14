﻿using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketVerifiersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class PacketVerifiersRepository : IPacketVerifiersRepository
    {
        private readonly Dictionary<PacketType, IPacketVerifier> _packetVerifiers;
        private readonly object _sync = new object();

        public PacketVerifiersRepository(IPacketVerifier[] packetVerifiers)
        {
            _packetVerifiers = new Dictionary<PacketType, IPacketVerifier>();

            foreach (var packetVerifier in packetVerifiers)
            {
                if(!_packetVerifiers.ContainsKey(packetVerifier.PacketType))
                {
                    _packetVerifiers.Add(packetVerifier.PacketType, packetVerifier);
                }
            }
        }

        public IPacketVerifier GetInstance(PacketType packetType)
        {
            if (!_packetVerifiers.ContainsKey(packetType))
            {
                throw new NotSupportedPacketTypeHandlerException(packetType);
            }

            return _packetVerifiers[packetType];
        }
    }
}
