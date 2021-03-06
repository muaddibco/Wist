﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Blockchain.Core.Interfaces
{
    [ServiceContract]
    public interface IPacketVerifiersRepository : IRepository<IPacketVerifier, PacketType>
    {
    }
}
