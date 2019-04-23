using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ExtensionPoint]
    public interface IConfigurationSection
    {
        string SectionName { get; }

        void Initialize();
    }
}
