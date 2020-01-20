using Newtonsoft.Json;
using System;
using System.IO;

namespace PowerHabbitsMonitoring
{
    public class StatusService
    {
        private System.Timers.Timer _timer;
        private Status _status = new Status();
        private DBSettingsProvider _settings;
        private SystemStats _systemStats;

        public StatusService(DBSettingsProvider settings)
        {
            _settings = settings;
            _systemStats = new SystemStats(settings);
        }

        public void Run()
        {
            _timer = new System.Timers.Timer(_settings.Default.IdleTimeCheckIntervalSeconds * 1000);
            _timer.Elapsed += (s, a) =>
            {
                Update();
            };
            _timer.Start();
            Update();
        }

        private void Update()
        {
            var inactiveTime = _systemStats.GetInactiveTime();
            if(inactiveTime != null)
            {
                if (inactiveTime.Value > _settings.Default.IdleTimeSeconds)
                {
                    InactiveActive(true);
                }
                else
                {
                    if (_status.Inactive.Value)
                    {
                        InactiveActive(false);
                    }
                }
            }            
            _status.Value += _systemStats.GetCorrectedTotalEnergy(_settings.Default.IdleTimeCheckIntervalSeconds);
            _timer.Interval = _settings.Default.IdleTimeCheckIntervalSeconds * 1000;
        }

        public void LockUnlock(bool locked)
        {
            if(_status.Locked != locked)
            {
                _status.EndTime = DateTime.Now;
                var json = JsonConvert.SerializeObject(_status);
                File.AppendAllText(Settings.Default.StatusCache, json + ",\n");
                _status.Value = 0;
                _status.Locked = locked;
                _status.StartTime = DateTime.Now;                
            }
        }

        public void SleepingAwake(bool sleeping)
        {
            if (_status.Sleeping != sleeping)
            {
                _status.EndTime = DateTime.Now;
                var json = JsonConvert.SerializeObject(_status);
                File.AppendAllText(Settings.Default.StatusCache, json + ",\n");
                _status.Value = 0;
                _status.Sleeping = sleeping;
                _status.StartTime = DateTime.Now;
            }
        }

        public void InactiveActive(bool inactive)
        {
            if (_status.Inactive != inactive)
            {
                _status.EndTime = DateTime.Now;
                var json = JsonConvert.SerializeObject(_status);
                File.AppendAllText(Settings.Default.StatusCache, json + ",\n");
                _status.Value = 0;
                _status.Inactive = inactive;
                _status.StartTime = DateTime.Now;
            }
        }
    }
}
