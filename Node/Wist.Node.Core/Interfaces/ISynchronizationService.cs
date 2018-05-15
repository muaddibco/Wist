using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface ISynchronizationService
    {
        void Initialize(CancellationToken cancellationToken);

        void Start();
    }
}
