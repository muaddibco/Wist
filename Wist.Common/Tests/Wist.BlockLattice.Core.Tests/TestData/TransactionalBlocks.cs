using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.Tests.TestData
{
    internal static class TransactionalBlocks
    {
        public static string GenesisBlock_1 => 
            "1002" + // DLE STX
            "____" + // Length
            "101200" + // CHAIN TYPE WITH DLE (Transactional)
            "101200" + // MESSAGE TYPE WITH DLE (Genesis)
            "ba62ff2a7b3b55c93541700b08b25df6d084a30e9ef476491f98087e407f611309c4f6726e2b4f66d36ac35384da3507d405d389883f5ba3be2a785e49322780" +
            "243b0d9593fe606f7484ca3c609a4edf6a682b2366fc19278c447989b92490ce9ebae9f9fff1b4b8bf72eb6d437cd4c6c088e9e50557d99149472e913ac92b77" +
            "" +
            "";
    }
}
