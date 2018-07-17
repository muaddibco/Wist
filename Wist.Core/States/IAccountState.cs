using Wist.Core.Identity;

namespace Wist.Core.States
{
    public interface IAccountState : IState
    {
        IKey AccountKey { get; }


        void Initialize();
    }
}
