using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ServiceContract]
    public interface IConfigurationService
    {
        IConfigurationSection this[string sectionName] { get; }

        T Get<T>(string sectionName) where T: class, IConfigurationSection;
    }
}
