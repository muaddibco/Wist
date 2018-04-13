using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Wist.Core.ExtensionMethods
{
    public static class ByteArrayExtensionMethods
    {
        public static string ToHexString(this byte[] arr)
        {
            if (arr != null)
            {
                return arr.Select(b => $"0x{b.ToString("X2")}").Aggregate((s, s1) => $"{s} {s1}");
            }
            return null;
        }
    }
}
