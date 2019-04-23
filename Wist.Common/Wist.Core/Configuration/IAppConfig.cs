using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ServiceContract]
    public interface IAppConfig
    {
        string GetString(string key, bool required = true);

        long GetLong(string key, bool required = true);

        bool GetBool(string key, bool required = true);
    }
}
