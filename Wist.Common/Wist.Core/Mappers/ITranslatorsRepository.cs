using Wist.Core.Architecture;

namespace Wist.Core.Translators
{
    [ServiceContract]
    public interface ITranslatorsRepository : IRepository<ITranslator, string, string>
    {
        ITranslator<TFrom, TTo> GetInstance<TFrom, TTo>();
    }
}
