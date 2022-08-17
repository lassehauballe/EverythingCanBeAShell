# Everything Is A(n asynchronous) Reverse Shell

This repository is made to explore some uncommon ways for establishing command and control (c2) on Microsoft Windows systems. 

The reverse shells in this repository are asynchronous just like Cobalt Strikes shells. Asyncronous, meaning that the client and server will not communicate all the time like a standard TCP reverse shell. Instead, the client will check-in for any new commands from the server. If a command is found, it will execute it and post the output back to the server. The checkin-interval is also just like Cobalt Strikes sleep timer. 

## C2 using Windows registry keys

### Short description: 
This reverse shell is based on Windows Registry keys. Windows allows administrators to read, write & delete registry keys on remote systems using the RPC protocol. Using  custom code, it is possible to create a client-server relationsship by having certain registry keys located on the server that the client can interact with.  

**Requirements:** 
The Remote Registry Service needs to be enabled on the server, and the user trying to connect/read/write to the remote registry needs to have the correct permissions.

**Limitations:** 
Since the traffic is based on RPC, should only be suitable for lateral movement on local networks. 

**Usage:**
```
RegC2Server.exe <Unique Registry key name to use>
RegC2Server.exe ws01
```

**How it works:**
When starting the server, the Remote Registry Service is enabled, and the following registry keys are created: cmd, output & sleep. 

### Screenshot:


Win32 apis 

Wireshark traffic

intro to shell, usage. 
