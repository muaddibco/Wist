using CommonServiceLocator;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Wist.Core.Architecture;
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
            UnityContainer unityContainer = (UnityContainer)ServiceLocator.Current.GetInstance<IUnityContainer>();
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
            unityContainer.RegisterInstance(appConfig);

            ApplicationContext applicationContext = new ApplicationContext() { Container = unityContainer};

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[2] { new ConfigA(applicationContext), new ConfigB(applicationContext) });
            
            ConfigA configA = (ConfigA)configurationService["configA"];
            ConfigB configB = (ConfigB)configurationService["configB"];
            configA.Initialize();
            configB.Initialize();

            ushort maxValueA = configA.MaxValue;
            ushort maxValueB = configB.MaxValue;

            Assert.Equal(10, maxValueA);
            Assert.Equal(20, maxValueB);
        }

        [Fact]
        public void ConfigurationSectionArrayValueTest()
        {
            UnityContainer unityContainer = (UnityContainer)ServiceLocator.Current.GetInstance<IUnityContainer>();
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                switch (ci.Arg<string>().ToLower())
                {
                    case "configroles:roles":
                        return "roleA, roleB";
                }

                return null;
            });
            unityContainer.RegisterInstance(appConfig);

            ApplicationContext applicationContext = new ApplicationContext() { Container = unityContainer };

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[1] { new ConfigRoles(applicationContext) });

            ConfigRoles configRoles = (ConfigRoles)configurationService["configroles"];
            configRoles.Initialize();
            Assert.Equal(2, configRoles.Roles.Length);
            Assert.Contains("roleA", configRoles.Roles);
            Assert.Contains("roleB", configRoles.Roles);
        }

        [Fact]
        public void ConfigurationSectionIntArrayValueTest()
        {
            UnityContainer unityContainer = (UnityContainer)ServiceLocator.Current.GetInstance<IUnityContainer>();
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                switch (ci.Arg<string>().ToLower())
                {
                    case "configints:ints":
                        return "5, 10";
                }

                return null;
            });
            unityContainer.RegisterInstance(appConfig);

            ApplicationContext applicationContext = new ApplicationContext() { Container = unityContainer };

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[1] { new ConfigInts(applicationContext) });

            ConfigInts configInts = (ConfigInts)configurationService["configints"];
            configInts.Initialize();

            Assert.Equal(2, configInts.Ints.Length);
            Assert.Contains(5, configInts.Ints);
            Assert.Contains(10, configInts.Ints);
        }
    }
}
