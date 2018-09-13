using CommonServiceLocator;
using Wist.Network;
using Wist.Network.Interfaces;
using Wist.Network.Communication;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Xunit;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Tests.Core.Fixtures;
using Wist.BlockLattice.Core.Handlers;

namespace Wist.Network.Tests.Fixtures
{
    public class DependencyInjectionFixture : DependencyInjectionSupportFixture
    {
        public DependencyInjectionFixture() : base()
        {
            BufferManager = new BufferManager();
            BufferManager.InitBuffer(200, 100);
        }

        public IPacketsHandler PacketsHandler { get; }

        public List<byte[]> Packets { get; }

        public IBufferManager BufferManager { get; set; }
    }
}
