using CommonServiceLocator;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Wist.Core.Configuration;
using Wist.Tests.Core.Fixtures;
using Xunit;

namespace Wist.Core.Tests
{
    public class AspectsTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        public AspectsTests()
        {
        }

        [Fact]
        public void ConfigurationSectionTest()
        {
            IUnityContainer unityContainer = ServiceLocator.Current.GetInstance<IUnityContainer>();
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                switch (ci.Arg<string>().ToLower())
                {
                    case "nodescommunication:maxconnections":
                        return "10";
                    case "accountscommunication:maxconnections":
                        return "20";
                }

                return null;
            });
            unityContainer.RegisterInstance<IAppConfig>(appConfig);

            ConfigurationService configurationService = ServiceLocator.Current.GetInstance<ConfigurationService>();

            ushort maxNodesConnections = configurationService.NodesCommunication.MaxConnections;
            ushort maxAccountsConnections = configurationService.AccountsCommunication.MaxConnections;

            Assert.Equal(10, maxNodesConnections);
            Assert.Equal(20, maxAccountsConnections);
        }
    }
}
