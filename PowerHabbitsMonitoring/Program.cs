using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                //var s = new PowerHabbitsMonitoring();
                //s.Start();
                //s.TestSend();
                //SystemStats.GetTotalMonitorWattUsage();
                var d = new Dictionary<string, double>();
                d.Add("dsa", 549);
                d.Add("bda", 569);
                var json = JsonConvert.SerializeObject(d);
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
