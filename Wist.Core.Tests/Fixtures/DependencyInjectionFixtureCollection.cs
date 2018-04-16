using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Wist.Core.Tests.Fixtures
{
    [CollectionDefinition("Dependency Injection")]
    public class DependencyInjectionFixtureCollection : ICollectionFixture<DependencyInjectionFixture>
    {
    }
}
