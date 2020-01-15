using System;
using System.ServiceProcess;

namespace PowerHabbitsMonitoring
{
    static class Program
    {
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                //Launched as console app
                var s = new PowerHabbitsMonitoring();
                s.Start();
                //s.TestSend();
                Console.ReadLine();

                
            }
            else
            {
                //Installed as a service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new PowerHabbitsMonitoring()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
