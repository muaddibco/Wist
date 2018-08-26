using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Core.Logging
{
    public interface ILogConfiguration : IConfigurationSection
    {
        bool MeasureTime { get; set; }

        string LogConfigurationFile { get; set; }
    }
}
