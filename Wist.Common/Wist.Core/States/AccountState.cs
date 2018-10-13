using System;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Core.States
{

    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class AccountState : IAccountState
    {
        private readonly Subject<string> _subject = new Subject<string>();
        private readonly ICryptoService _cryptoService;

        public AccountState(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public IKey AccountKey { get; private set; }

        public string Name => nameof(IAccountState);

        public void Initialize()
        {
            AccountKey = _cryptoService.PublicKey;
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}
