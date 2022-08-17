# Everything Is A(n asynchronous) Reverse Shell

Repository made to explore uncommon ways for establishing command and control (c2) on Windows systems. The reverse shells in this repository are created on the same concept as Cobalt Strikes shells. They are asyncronous meaning that the client checks in on the server for a new command at a specific time, and if there are any new commands it will execute it and post the output back to the server. 

## C2 using Windows registry keys

### Short description: 
Windows allows administrators to read, write & delete registry keys on remote systems. To do so, the Windows Service: Remote Registry Service needs to be enabled, and the user trying to connect/read/write to the remote database needs to have the correct permissions. 

Win32 apis 

Wireshark traffic

intro to shell, usage. 
