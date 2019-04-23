# Storage Layer flows

## Processing Storage Transactions
Transactions are stored into mempool until an updated combined block from Transactions 
Registration Layer is obtained.

Once an updated combined block obtained full transaction blocks registered there are 
stored into database.

**StorageHandler**

Handler that receives full transactions, stores them into mempool and distributes the 
INV packet over the network.

INV (*INV*entory) Packet is packet that consists out of MurMur hash of transaction. 
This way nodes of Storage Layer always can 

Other nodes are checking INV packets and verifying they have reported transaction.

**StorageRegistryHandler**

Processes combined blocks from Registry Layer as follows:

1. Once combined block obtained it downloads all TransactionsRegistrationFullBlocks, if not obtained yet.
2. Checks what transactions are registered in them and stores into Database

### MVP Stage
At MVP Stage StorageRegistryHandler won't be implemented and StorageHandler will save transactions to database immediately