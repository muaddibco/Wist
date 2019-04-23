using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Text;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    internal class RegistrationSettings
    {
        internal AggregateCatalog MefInitialCatalog { get; set; }
        internal RunMode RunMode { get; set; }
    }
}
