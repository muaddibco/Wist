using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.States
{
    public interface IAccountState : IState
    {
        byte[] PublicKey { get; }

        void Initialize();
    }
}
