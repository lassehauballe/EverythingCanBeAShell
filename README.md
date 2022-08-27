# Everything can be a shell... An asynchronous shell at least...


### Disclaimer
The shells in this repository are all Proof-of-Concept and have only been tested in my own lab. Enjoy :)  

*Also, at this moment, the repository only contains a single shell based on Windows Registry keys. However, I do hope to add more in the future.*  

### Concept
This repository is made to explore untraditional ways of establishing command and control (C2) on Microsoft Windows systems and to keep the SOC on its toes. The shells in this repository are asynchronous just like Cobalt Strikes shells. The client (beacon) will check-in at a given interval (sleep-timer) for any new commands from the server (C2). If a command is found, the beacon will execute it and post the result back to the server. 

# Shell 1: C2 using Windows registry keys
This shell is based on Windows Registry keys. Yes, registry keys as in regedit.exe. The idea comes from the fact, that Windows allows for users (with the correct permissions) to read, write and/or delete registry keys and values on remote systems using the WinReg Protocol (MS-RRP). The "protocol" is built on top of the RPC protocol and is described by Microsoft: (https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-rrp/0fa3191d-bb79-490a-81bd-54c2601b7a78). Using custom code, it is possible to create a client-server-relationsship by having certain registry keys located on the server that the client can interact with.  

