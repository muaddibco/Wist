using System.Threading;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Handlers
{
    /// <summary>
    /// Service receives raw arrays of bytes representing all types of messages exchanges over network. 
    /// Byte arrays must contain exact bytes of message to be processed correctly.
    /// </summary>
    [ServiceContract]
    public interface IPacketsHandler
    {
        void Initialize(CancellationToken ct);

        /// <summary>
        /// Bytes being pushed to <see cref="IPacketsHandler"/> must form complete packet for following validation and processing
        /// </summary>
        /// <param name="messagePacket">Bytes of complete message for following processing</param>
        void Push(byte[] messagePacket);

        void Start();
    }
}
