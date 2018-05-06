using CommonServiceLocator;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Wist.Core.Configuration;
using Wist.Core.Tests.Classes;
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
                    case "configa:maxvalue":
                        return "10";
                    case "configb:maxvalue":
                        return "20";
                }

                return null;
            });
            unityContainer.RegisterInstance<IAppConfig>(appConfig);
            unityContainer.RegisterType<IConfigurationSection, ConfigA>();
            unityContainer.RegisterType<IConfigurationSection, ConfigB>();

            ConfigurationService configurationService = ServiceLocator.Current.GetInstance<ConfigurationService>();

            ConfigA configA = (ConfigA)configurationService["configA"];
            ConfigB configB = (ConfigB)configurationService["configB"];
            ushort maxValueA = configA.MaxValue;
            ushort maxValueB = configB.MaxValue;

            Assert.Equal(10, maxValueA);
            Assert.Equal(20, maxValueB);
        }
    }
}
