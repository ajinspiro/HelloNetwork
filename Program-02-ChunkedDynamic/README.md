# Program 02 - ChunkedDynamic
### Overview
This program builds upon the foundation laid in Program 1, addressing several of its limitations. In the original implementation, file metadata was hardcoded, and the receiver was configured to read exactly 90,674 bytes from the channel. As a result, the system was limited to transferring only a specific file, which significantly reduced its practical utility.

Additionally, Program 1 transmitted the entire file in a single write operation, making error recovery both costly and inefficient. In this enhanced version, we introduce support for transferring files of arbitrary size. We also implement chunked transfer, which improves reliability and reduces the cost of error recovery in the event of packet loss.


```mermaid
sequenceDiagram
    participant Client
    participant Server

Client<<->>Server:TCP conn. established
Client->>Server:Metadata of file
Client<<-Server:Proceed (or Denied if not enough disk space)
Client->>Server:Chunk 1 (4KB) of file
Client<<-Server:Proceed
Client->>Server:Chunk 2 (4KB) of file
Client<<-Server:Proceed
Client->>Server:End of file
Client<<->>Server:TCP conn. closed
```

### Current Limitations of the Program
* **Lack of security**: Data is sent as unencrypted bits over TCP, making it vulnerable to interception and inspection.