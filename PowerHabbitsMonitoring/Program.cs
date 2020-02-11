using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
//2020-01-17T11:39:18.8469574+02:00
namespace PowerHabbitsMonitoring
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static void ConfigureLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "error.txt", Layout = "${longdate} ${message} ${exception:format=tostring}" };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        static void Main()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            ConfigureLogger();
            _logger.Info("Application started");

            if (Environment.UserInteractive)
            {
                _logger.Info("Running in console mode.");
                var s = new PowerHabbitsMonitoring();
                s.Start();
            }
            else
            {
                try
                {
                    //Installed as a service
                    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                    new PowerHabbitsMonitoring()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                catch(Exception e)
                {
                    _logger.Error(e, "Exception in main.");
                }
            }
        }
    }
}
