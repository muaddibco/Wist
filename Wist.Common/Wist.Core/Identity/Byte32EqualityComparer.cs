using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Identity
{
	public class Byte32EqualityComparer : IEqualityComparer<byte[]>
	{
		public bool Equals(byte[] x, byte[] y)
		{
			if(x != null && y != null)
			{
				if (x.Length == y.Length && x.Length == 32)
				{
					return x.Equals32(y);
				}

				if(x.Length != 32)
				{
					throw new ArgumentOutOfRangeException(nameof(x));
				}

				if (y.Length != 32)
				{
					throw new ArgumentOutOfRangeException(nameof(y));
				}
			}

			return false;
		}

		public int GetHashCode(byte[] obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			if(obj.Length != 32)
			{
				throw new ArgumentOutOfRangeException(nameof(obj));
			}

			return obj.GetHashCode32();
		}
	}
}
