using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ServiceContract]
    public interface IConfigurationService
    {
        IConfigurationSection this[string sectionName] { get; }

        T Get<T>() where T: class, IConfigurationSection;
    }
}
