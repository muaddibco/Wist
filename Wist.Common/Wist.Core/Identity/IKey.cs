using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Identity
{
    /// <summary>
    /// Generic key
    /// </summary>
    public interface IKey : IEqualityComparer<IKey>, IEquatable<IKey>
    {
        int Length { get; }

        Memory<byte> Value { get; set; }

        ArraySegment<byte> ArraySegment { get; }
    }
}
