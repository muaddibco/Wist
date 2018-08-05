using Wist.Core.Architecture;

namespace Wist.Core.States
{
    [ServiceContract]
    public interface IStatesRepository : IRepository<IState, string>
    {
        T GetInstance<T>() where T : class, IState;
    }
}
