using Newtonsoft.Json;
using NLog;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DBService(DBSettingsProvider settings)
        {
            _settings = settings;            
        }

        public void Run()
        {
            try
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
            catch(Exception e)
            {
                _logger.Error(e, "Error starting db service");
            }            
        }

        private void Update()
        {
            try
            {
                _settings.Update();
                if (File.Exists("StatusCache.txt"))
                {
                    var json = "[" + File.ReadAllText("StatusCache.txt") + "]";
                    var statuses = JsonConvert.DeserializeObject<List<Status>>(json);
                    json = JsonConvert.SerializeObject(statuses);
                    using (var client = new HttpClient())
                    {
                        var rez = client.PostAsync("http://ws000webdev1.tcad.telia.se/PowerConsumptionMonitor/api/PowerConsumption", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                    }
                    File.Delete("StatusCache.txt");
                }
                _timer.Interval = _settings.Default.DBSendIntervalHours * 3600000;
                _lastWriteTime = DateTime.Now;
            }
            catch(Exception e)
            {
                _logger.Error(e, "Error updating database.");
            }            
        }

        public void SendIfNeedsSending()
        {
            try
            {
                if (DateTime.Now - TimeSpan.FromHours(_settings.Default.DBSendIntervalHours) > _lastWriteTime)
                {
                    Update();
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "Error checking if needs sending.");
            }            
        }
    }
}
