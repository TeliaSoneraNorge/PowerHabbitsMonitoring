using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
//2020-01-17T11:39:18.8469574+02:00
namespace PowerHabbitsMonitoring
{
    static class Program
    {
        public static void StartActivityLogger()
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = Settings.Default.ActiveStatusExe;
                    myProcess.StartInfo.CreateNoWindow = false;
                    myProcess.Start();
                }
            }
            catch (Exception e)
            {
                //Rethrow, application should not start.
                throw e;
            }
        }

        static void Main()
        {
            if (Environment.UserInteractive)
            {
                StartActivityLogger();
                var s = new PowerHabbitsMonitoring();
                s.Start();
                Console.ReadLine();
            }
            else
            {
                try
                {
                    //Installed as a service
                    StartActivityLogger();
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                    new PowerHabbitsMonitoring()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                catch(Exception e)
                {
                    File.WriteAllText(@"C:\Users\dno1694\source\repos\PowerHabbitsMonitoring\PowerHabbitsMonitoring\bin\Debug\error.txt", e.Message);
                }
                
            }
        }
    }
}
