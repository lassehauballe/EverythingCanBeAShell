# Everything Is A(n asynchronous) Reverse Shell

This repository is made to explore some uncommon ways for establishing command and control (c2) on Microsoft Windows systems. 

The reverse shells in this repository are asynchronous just like Cobalt Strikes shells. Asyncronous, meaning that the client and server will not communicate all the time like a standard TCP reverse shell. Instead, the client will check-in for any new commands from the server. If a command is found, it will execute it and post the output back to the server. The checkin-interval is also just like Cobalt Strikes sleep timer. 

## C2 using Windows registry keys

### Short description: 
This reverse shell is based on Windows Registry keys. Windows allows administrators to read, write & delete registry keys on remote systems using the RPC protocol. Using  custom code, it is possible to create a client-server relationsship.  

**Requirements:** The Remote Registry Service needs to be enabled on the server, and the user trying to connect/read/write to the remote registry needs to have the correct permissions.

**Limitations:** Since the traffic is based on RPC, the shell is suitable for lateral movement on local networks. 

Win32 apis 

Wireshark traffic

intro to shell, usage. 
