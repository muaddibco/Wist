# Wist
>Faber est suae quisque fortunae - Each man is the maker of his own fortune

(Guy Sallust Crisp)


Wist is an experimental platform with new blockchain architecture build from scratch and sharpened for unlimited scaling 
without sacrificing decentralization and with balanced incentivising of functional network participants.

## Core Principles
1. The consensus is not achieved in the meaning of transaction content but only on the fact that the transaction had the place
   -  The only thing that matters is the immutability of history
   -  Every receiver is responsible to ascertain that transactions being accepted are correct and valid
2. There is global blockchain that defines uniqueness and immutability of history and is the only source of 
truth for identifying what transaction were passed over a network for all network participants. This blockchain does store transactions of participants in their entirety but only so-called "Transaction Witnesses":
   - For Account/Balance model based transactions hash and Height of every transaction and Public Key of issuer stored in that global blockchain where Public Key of issuer and transaction's Height have to be unique. 
   - For UTXO model based transactions hash and Key Image of every transaction is stored in that global blockchain where Key Image have to be unique.
3. There is a mechanism of synchronization that is running on its own global blockchain and every message passed over a network must have reference to the latest synchronization block otherwise it will be rejected. Besides reference, certain types of messages must have proof of work based on referenced synchronization as part of a message in order to provide defense against spam attacks.

## Network Architecture
Network has a four-layer structure:
1. Storage Layer - layer of nodes responsible for storing content of transactions
2. Registration Layer - layer of nodes responsible for creating blockchains of registration of transactions 
3. Synchronization Layer - layer of nodes responsible for:
   - creating Synchronization Blocks 
   - combining blockchains of registration of transactions from shards into unified blockchain in order to ascertain History Immutability
4. Deferred Consensus Layer - layer of nodes that are validating transaction registered by Registration Layer and creating final version of Blockchain

### Scheme of responsibilities of Network Layers

![](https://github.com/muaddibco/Wist/blob/master/Wist%20Layers.png?raw=true)

## Transactions Flow

__TBD__

## Network Economy

__TBD__

## How to run Node
### 1. Performance Counters initialization
If Node was not running on current workstation it is needed to launch Wist.Setup.exe that is responsible for creating Windows Performance Counters. This executable located at "Wist.Node\NodeSetup\Wist.Setup". Launch it "As Administrator" and wait until it will finish.

### 2. Database initialization
Before running Node it is needed to initialize database. Launch Wist.Setup.Simulation.exe "As Administrator" with argument --WipeAll. This executable located at "Wist.Node\NodeSetup\Wist.Setup.Simulation". Do not close window when it will finish - copy aside key printed in console window and only then close window.

### 3. Running Node
In order to run Node launch Wist.Node.Console.exe and provide it with key copied aside during database initialization step.