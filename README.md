# Wist
>Faber est suae quisque fortunae - Each man is the maker of his own fortune

(Guy Sallust Crisp)


Wist is an experimental platform with new blockchain architecture build from scratch and sharpened for unlimited scaling 
without sacrificing decentralization and with balanced incentivising of functional network participants.

## Core Principles
1. Every account has it's own independent private blockchain that can operate with one type of data. 
2. Consensus is not achieved in meaning of transaction content but only on fact that transaction had place
   -  The only thing that matters is the immutability of history
   -  Every receiver is responsible to ascertain that transactions being accepted are correct and valid
3. Hash and Height of every block of transaction and Public Key of issuer gets stored in dedicated global blockchain where 
Public Key of issuer and transaction block's Height have to be unique. This global blockchain serves for documenting uniqueness and immutability 
of history and is the only source of truth for identifying what transaction were passed over network for all network participants.
4. There is mechanism of synchronization that is running on its own global blockchain and every message passed over network 
must have reference to the latest synchronization block otherwise it will be rejected. Besides reference certain types of messages must have 
proof of work based on referenced synchronization as part of message in order to provide defense against spam attacks.
5. Any private blockchain is stored only on part of network nodes rather than on all of them. 
All other nodes are storing only one's latest state, documented in the blockchain of history uniqueness.

## Network Architecture
Network has a three-layer structure:
1. Layer of nodes (_Full Nodes_) responsible for storing private blockchains of transactions
2. Layer of nodes (_Master Nodes_) responsible for creating blockchains of registration of transactions of private blockchains
3. Layer of nodes (_Synchronization Nodes_) responsible for:
   - creating Synchronization Blocks 
   - combining blockchains of registration of transactions of private blockchains into unified blockchain in order to ascertain History Immutability
   - deferred transactions content consensus

### Scheme of responsibilities of Network Layers

![](https://github.com/muaddibco/Wist/blob/master/Wist%20Layers.png?raw=true)

## Transactions Flow
### Synchronization Blocks Producing

__TBD__

### Sending funds to another account
1. Account peeks latest Synchronization Block broadcasted over network
2. Calculates POW basing on account's Public Key and Synchronization Block hash
3. Composes block of Transaction with all details
    - Reference to latest Sync Block
    - Calculated POW (see #2)
    - Height of Transaction Block
    - Content of Transaction Block
    - Account's Public Key 
    - Signature
4. Composes description of Transaction Block that includes:
    - Reference to latest Sync Block
    - Calculated POW (see #2)
    - Height of Transaction Block
    - Hash of Transaction Block
    - Account's Public Key 
    - Signature
5. Sends description of Transaction Block to corresponding group of Master Nodes and Full Nodes with purpose to register Transaction in Blockchain of Transactions Registration
6. Sends Transaction Block to corresponding group of Full Nodes that includes:
    - Full Nodes that are responsible for storing private blockchain of sender
    - Full Nodes that are responsible for storing private blockchain of receiver

### Accepting funds from another account

__TBD__

## Network Economy

__TBD__