using Wist.Core.Architecture;

namespace Wist.Core.Predicates
{
    [ServiceContract]
    public interface IPredicatesRepository : IRepository<IPredicate, string>
    {
    }
}
