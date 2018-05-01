﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IBlockParser
    {
        BlockType BlockType { get; }

        BlockBase Parse(byte[] source);

        void FillBlockBody(BlockBase block, byte[] blockBody);
    }
}
