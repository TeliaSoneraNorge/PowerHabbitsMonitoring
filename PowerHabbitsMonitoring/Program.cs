using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

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
