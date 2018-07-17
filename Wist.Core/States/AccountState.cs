using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Core.States
{

    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class AccountState : IAccountState
    {
        private readonly Subject<string> _subject = new Subject<string>();

        public IKey AccountKey { get; private set; }

        public string Name => nameof(AccountState);

        public void Initialize()
        {
            
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}
