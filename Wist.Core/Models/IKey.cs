using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Models
{
    /// <summary>
    /// Generic key
    /// </summary>
    public interface IKey : IEqualityComparer<IKey>
    {
        byte[] Value { get; set; }
    }
}
