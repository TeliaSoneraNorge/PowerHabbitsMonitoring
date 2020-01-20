using System;
using System.ServiceProcess;
//2020-01-17T11:39:18.8469574+02:00
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
