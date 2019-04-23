# Process of Node Start Up
**Prerequisites**
- Either configuration file or dedicated table contains initial list of Nodes with corresponding DNS names and/or IPs

**Initialization flow**

1. Read initial list of Nodes from DB or Configuration File
2. Get height of latest block of Root Chain that contains hashes of all transactions
3. Check against known nodes latest height and download missing part
4. Scan Root Chain for all Nodes registered there
