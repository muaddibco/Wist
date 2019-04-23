﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Registry
{
    public interface IRegistryConfiguration : IConfigurationSection
    {

        string TcpServiceName { get; set; }

        string UdpServiceName { get; set; }

        int ShardId { get; set; }

        int TotalNodes { get; set; }

        int Position { get; set; }
    }
}
