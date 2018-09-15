# Storage Layer flows

## Processing Storage Transactions
Transactions are stored into mempool until an updated combined block from Transactions 
Registration Layer is obtained.

Once an updated combined block obtained full transaction blocks registered there are 
stored into database.

**StorageHandler**

Handler that receives full transactions, stores them into mempool and distributes the 
INV packet over the network.

**StorageRegistryHandler**

Processes combined blocks from Registry Layer