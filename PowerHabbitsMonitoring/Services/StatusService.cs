using Newtonsoft.Json;
using NLog;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public StatusService(DBSettingsProvider settings)
        {
            _settings = settings;
            _systemStats = new SystemStats(settings);
        }

        public void Run()
        {
            try
            {
                _timer = new System.Timers.Timer(_settings.Default.IdleTimeCheckIntervalSeconds * 1000);
                _timer.Elapsed += (s, a) =>
                {
                    Update();
                };
                _timer.Start();
                Update();
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception starting running StatusService.");
            }            
        }

        private void Update()
        {
            try
            {
                var inactiveTime = _systemStats.GetInactiveTime();
                if (inactiveTime != null)
                {
                    if (inactiveTime.Value > _settings.Default.IdleTimeSeconds)
                    {
                        InactiveActive(true);
                    }
                    else
                    {
                        InactiveActive(false);
                    }
                }
                else
                {
                    InactiveActive(null);
                }
                _status.Value += _systemStats.GetCorrectedTotalEnergy(_settings.Default.IdleTimeCheckIntervalSeconds);
                _timer.Interval = _settings.Default.IdleTimeCheckIntervalSeconds * 1000;
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception updating StatusService.");
            }            
        }

        public void LockUnlock(bool locked)
        {
            try
            {
                if (_status.Locked != locked)
                {
                    _status.EndTime = DateTime.Now;
                    var json = JsonConvert.SerializeObject(_status);
                    File.AppendAllText("StatusCache.txt", json + ",\n");
                    _status.Value = 0;
                    _status.Locked = locked;
                    _status.StartTime = DateTime.Now;
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception changing status locking/unlocking.");
            }
        }

        public void SleepingAwake(bool sleeping)
        {
            try
            {
                if (_status.Sleeping != sleeping)
                {
                    _status.EndTime = DateTime.Now;
                    var json = JsonConvert.SerializeObject(_status);
                    File.AppendAllText("StatusCache.txt", json + ",\n");
                    _status.Value = 0;
                    _status.Sleeping = sleeping;
                    _status.StartTime = DateTime.Now;
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception changing status sleeping/awaking.");
            }
        }

        public void InactiveActive(bool? inactive)
        {
            try
            {
                if (_status.Inactive != inactive)
                {
                    _status.EndTime = DateTime.Now;
                    var json = JsonConvert.SerializeObject(_status);
                    File.AppendAllText("StatusCache.txt", json + ",\n");
                    _status.Value = 0;
                    _status.Inactive = inactive;
                    _status.StartTime = DateTime.Now;
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception changing status active/inactive.");
            }
        }
    }
}
