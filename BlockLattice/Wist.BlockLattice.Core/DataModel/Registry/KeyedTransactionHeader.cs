using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class KeyedTransactionHeader
    {
        public KeyedTransactionHeader()
        {

        }

        public KeyedTransactionHeader(IKey key, TransactionHeader transactionHeader)
        {
            Key = key;
            TransactionHeader = transactionHeader;
        }

        public IKey Key { get; set; }

        public TransactionHeader TransactionHeader { get; set; }
    }
}
