using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PowerHabbitsMonitoring
{
    public class DBSettingsProvider
    {
        public DBSettings Default;

        public DBSettingsProvider()
        {
            Update();
        }

        public void Update()
        {
            using (var httpClient = new HttpClient())
            {
                var rez = httpClient.GetAsync(Settings.Default.ApiURL + $"/{Environment.MachineName}").Result;
                var json = rez.Content.ReadAsStringAsync().Result;
                Default = JsonConvert.DeserializeObject<DBSettings>(json);
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
