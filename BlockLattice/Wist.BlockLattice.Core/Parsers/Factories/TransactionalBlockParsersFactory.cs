﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockParsersFactory : BlockParsersFactoryBase
    {
        public TransactionalBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override ChainType ChainType => ChainType.TransactionalChain;
    }
}