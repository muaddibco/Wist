﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IBlockParsersFactory : IFactory<IBlockParser, ushort>
    {
        ChainType ChainType { get; }
    }
}