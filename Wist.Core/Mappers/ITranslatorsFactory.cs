using Wist.Core.Architecture;

namespace Wist.Core.Translators
{
    [ServiceContract]
    public interface ITranslatorsFactory
    {
        ITranslator<TFrom, TTo> GetTranslator<TFrom, TTo>();
    }
}
