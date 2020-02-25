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


        private DateTime _firstInputDetectedTime = DateTime.Now;
        private bool _firstInputDetected = false;


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
                _logger.Debug("Updating active status...");
                var inactiveTime = _systemStats.GetInactiveTime();
                if (inactiveTime != null)
                {
                    if (inactiveTime.Value < _settings.Default.IdleTimeSeconds)
                    {
                        if(!_firstInputDetected)
                        {
                            _firstInputDetected = true;
                            _firstInputDetectedTime = DateTime.Now;
                        }
                        else
                        {
                            if(_status.Inactive.Value)
                            {
                                var secondsSinceFirstInput = (DateTime.Now - _firstInputDetectedTime).Seconds;
                                var activeTime = secondsSinceFirstInput - inactiveTime;
                                var activeTimeLimit = _settings.Default.IdleTimeCheckIntervalSeconds;
                                var activeTimeResetLimit = _settings.Default.IdleTimeCheckIntervalSeconds * 2;

                                if (activeTime > activeTimeLimit)
                                {
                                    _logger.Debug($"Detected multiple inputs in the last {secondsSinceFirstInput}seconds, setting user to active.");
                                    InactiveActive(false);
                                }
                                else
                                {
                                    if(secondsSinceFirstInput > activeTimeResetLimit)
                                    {
                                        _logger.Debug($"Did not detect multiple inputs for the last {secondsSinceFirstInput}seconds. Resetting this timer.");
                                        _firstInputDetected = false;
                                    }
                                }
                            }                            
                        }                        
                    }
                    else
                    {
                        _firstInputDetected = false;
                        _logger.Debug($"Status is inactive, because {inactiveTime.Value}s is more than {_settings.Default.IdleTimeSeconds}s.");
                        InactiveActive(true);
                    }
                }
                else
                {
                    _logger.Debug($"Inactive time is null.");
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
                _logger.Debug($"Lock event detected system should be locked: {locked}.");
                if (_status.Locked != locked)
                {
                    _logger.Debug($"System was locked: {_status.Locked}, changing to {locked}.");
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
                _logger.Debug($"Power event detected system should be sleeping: {sleeping}.");
                if (_status.Sleeping != sleeping)
                {
                    _logger.Debug($"System was sleeping: {_status.Sleeping}, changing to {sleeping}.");
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
                    _logger.Debug($"Status was inactive :{_status.Inactive}, now changing it to inactive : {inactive}");
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
