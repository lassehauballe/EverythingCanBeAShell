# Everything Is A(n asynchronous) Reverse Shell

Repository made to explore uncommon ways for establishing command and control (c2) on Windows systems. The reverse shells in this repository are asynchronous just like Cobalt Strikes shells. Asyncronous, meaning that the client will not execute the command immidiately, but will instead check-in for any new commands from the server. If new commands are found it will execute it and post the output back to the server. The checkin-interval (sleep timer) can be changed, however it will not be possible to make them instantaneous/interactive. 

## C2 using Windows registry keys

### Short description: 
The following reverse shell is based on Windows Registry keys. Windows allows administrators to read, write & delete registry keys on remote systems, thus using custom code, it is possible to create a client and server relationsship. To do so, the Windows Service: Remote Registry Service needs to be enabled, and the user trying to connect/read/write to the remote database needs to have the correct permissions. 

Win32 apis 

Wireshark traffic

intro to shell, usage. 
