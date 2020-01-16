using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace PowerHabbitsMonitoring
{
    public static class DBSettingsProvider
    {
        public static DBSettings Default;
        public static void Update()
        {
            using (var httpClient = new HttpClient())
            {
                var rez = httpClient.GetAsync(Settings.Default.GetUrl).Result;
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
        public Dictionary<string, double> MonitorWattUsage { get; set; }        
    }
}
