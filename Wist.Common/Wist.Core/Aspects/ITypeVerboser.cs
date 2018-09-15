using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Aspects
{
    interface ITypeVerboser
    {
        string Verbose(object ex);
    }
}
