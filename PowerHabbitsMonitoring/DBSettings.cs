using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PowerHabbitsMonitoring
{
    public class DBSettingsProvider
    {
        public DBSettings Default;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DBSettingsProvider()
        {
            Update();
        }

        public void Update()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var rez = httpClient.GetAsync("http://ws000webdev1.tcad.telia.se/PowerConsumptionMonitor/api/PowerConsumption" + $"/{Environment.MachineName}").Result;
                    var json = rez.Content.ReadAsStringAsync().Result;
                    Default = JsonConvert.DeserializeObject<DBSettings>(json);
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "Error updating settings from database");
            }            
        }
    }

    public class DBSettings
    {
        public int DBSendIntervalHours { get; set; }
        public int DBSendOnStartMinDelayMinutes { get; set; }
        public int DBSendOnStartMaxDelayMinutes { get; set; }
        public int IdleTimeSeconds { get; set; }
        public int IdleTimeCheckIntervalSeconds { get; set; }
        public double DefaultMonitorWattUsage { get; set; } = 7.5;
        public Dictionary<string, double> MonitorWattUsage { get; set; }        
    }
}
