﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class AccountsCommunicationConfiguration : CommunicationConfigurationBase
    {
        public const string SECTION_NAME = "accountsCommunication";
        public override string SectionName => SECTION_NAME;
    }
}