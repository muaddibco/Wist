using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary
{
    public class ResponseFactoryEqualityComparer : IEqualityComparer<IMessageFactory>
    {
        public bool Equals(IMessageFactory x, IMessageFactory y)
        {
            return x.MessageCode == y.MessageCode;
        }

        public int GetHashCode(IMessageFactory obj)
        {
            return obj.MessageCode;
        }
    }
}
