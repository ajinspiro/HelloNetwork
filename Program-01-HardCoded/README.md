# Program 01 - HardCoded
This program introduces the reader to network programming. High level/course grained APIs are used to send a file from client to server. Details of the file is hard coded into client and server for simplicity which means the program can send that 1 file only.
```mermaid
sequenceDiagram
    participant Client as 📱 Client
    participant Server as 🖥️ Server

Client->Server:Solid line without arrow
Client-->Server:Dotted line without arrow
Client->>Server:Solid line with arrowhead
Client-->>Server:Dotted line with arrowhead
Client-xServer:Solid line with a cross at the end
Client--xServer:Dotted line with a cross at the end.
Client-)Server:Solid line with an open arrow at the end (async)
Client--)Server:Dotted line with a open arrow at the end (async)

```