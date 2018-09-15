using Wist.Core.Architecture;

namespace Wist.Core.Predicates
{
    [ExtensionPoint]
    public interface IPredicate
    {
        string Name { get; }

        bool Evaluate(params object[] args);
    }
}
