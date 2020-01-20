using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PowerHabbitsMonitoring
{
    public class DBService
    {
        private Timer _timer;
        private DBSettingsProvider _settings;
        private DateTime _lastWriteTime = DateTime.Now;

        public DBService(DBSettingsProvider settings)
        {
            _settings = settings;            
        }

        public void Run()
        {
            var rand = new Random();
            var firstSendDelay = rand.Next(_settings.Default.DBSendOnStartMinDelayMinutes, _settings.Default.DBSendOnStartMaxDelayMinutes) * 60 * 1000;
            Task.Delay(firstSendDelay).ContinueWith(t =>
            {
                Update();
            });
            _timer = new Timer(_settings.Default.DBSendIntervalHours * 3600000);
            _timer.Elapsed += (s, a) =>
            {
                Update();
            };
            _timer.Start();
        }

        private void Update()
        {
            _settings.Update();
            if (File.Exists(Settings.Default.StatusCache))
            {
                var json = "[" + File.ReadAllText(Settings.Default.StatusCache) + "]";
                var statuses = JsonConvert.DeserializeObject<List<Status>>(json);
                json = JsonConvert.SerializeObject(statuses);
                using (var client = new HttpClient())
                {
                    var rez = client.PostAsync(Settings.Default.ApiURL, new StringContent(json, Encoding.UTF8, "application/json")).Result;
                }
                File.Delete(Settings.Default.StatusCache);
            }
            _timer.Interval = _settings.Default.DBSendIntervalHours * 3600000;
            _lastWriteTime = DateTime.Now;
        }

        public void SendIfNeedsSending()
        {
            if(DateTime.Now - TimeSpan.FromHours(_settings.Default.DBSendIntervalHours) > _lastWriteTime)
            {
                Update();
            }
        }
    }
}
