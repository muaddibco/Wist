using Wist.Core.Architecture;

namespace Wist.Core.PerformanceCounters
{
    [ServiceContract]
    public interface IPerformanceCountersRepository : IRepository<IPerformanceCountersCategoryBase, string>
    {
        T GetInstance<T>() where T : class, IPerformanceCountersCategoryBase;
    }
}
