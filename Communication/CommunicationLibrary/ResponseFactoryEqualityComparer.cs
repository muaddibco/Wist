using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary
{
    public class ResponseFactoryEqualityComparer : IEqualityComparer<IResponseFactory>
    {
        public bool Equals(IResponseFactory x, IResponseFactory y)
        {
            return x.OpCode == y.OpCode;
        }

        public int GetHashCode(IResponseFactory obj)
        {
            return obj.OpCode;
        }
    }
}
