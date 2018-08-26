using Wist.Core.Configuration;

namespace Wist.Node.Core.Synchronization
{
    public interface ISynchronizationConfiguration : IConfigurationSection
    {
        string CommunicationServiceName { get; set; }
    }
}
