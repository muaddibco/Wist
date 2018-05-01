using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ServiceContract]
    public interface IConfigurationService
    {
        ICommunicationConfigurationService NodesCommunicationConfiguration { get; }
        ICommunicationConfigurationService AccountsCommunicationConfiguration { get; }
    }
}
