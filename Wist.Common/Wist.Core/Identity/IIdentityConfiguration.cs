using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Core.Identity
{
    public interface IIdentityConfiguration : IConfigurationSection
    {
        string Provider { get; set; }
    }
}
