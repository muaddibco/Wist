using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DAL.Keys
{
    public class DoubleHeightKey : IDataKey
    {
        public DoubleHeightKey(ulong height1, ulong height2)
        {
            Height1 = height1;
            Height2 = height2;
        }

        public ulong Height1 { get; set; }
        public ulong Height2 { get; set; }
    }
}
