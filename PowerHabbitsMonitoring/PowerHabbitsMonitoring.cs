using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace PowerHabbitsMonitoring
{
    public partial class PowerHabbitsMonitoring : ServiceBase
    {
        private System.Timers.Timer _inactiveTimer;
        private System.Timers.Timer _dbTimer;
        private double _tdp = 18;
        private Status _status = new Status();

        public PowerHabbitsMonitoring()
        {
            InitializeComponent();
        }

        public void TestSend()
        {
            UpdateDatabase();
        }

        public void Start()
        {
            OnStart(new string[] { });
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                SystemStats.GetPowerUsageSinceLastQuery();
                SystemStats.GetTDP(0, ref _tdp);

                Update();
                _inactiveTimer = new System.Timers.Timer(DBSettingsProvider.Default.IdleTimeCheckIntervalSeconds * 1000);
                _inactiveTimer.Elapsed += (s, e) =>
                {
                    Update();
                };
                _inactiveTimer.Start();

                var rand = new Random();
                Task.Delay(rand.Next(DBSettingsProvider.Default.DBSendOnStartMinDelayMinutes, DBSettingsProvider.Default.DBSendOnStartMaxDelayMinutes) * 60 * 1000).ContinueWith(t =>
                {
                    UpdateDatabase();
                });
                _dbTimer = new System.Timers.Timer(DBSettingsProvider.Default.DBSendIntervalHours * 3600 * 1000);
                _dbTimer.Elapsed += (s, e) =>
                {
                    UpdateDatabase();
                };
                _dbTimer.Start();
            }
            catch (Exception e)
            {
                Logging.Log(e.Message);
            }
        }

        protected override void OnStop()
        {
            
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLock:
                    ChangeStatus(true, _status.Sleeping, _status.Inactive);
                    break;

                case SessionChangeReason.SessionUnlock:
                    ChangeStatus(false, _status.Sleeping, _status.Inactive);
                    break;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    ChangeStatus(_status.Locked, true, _status.Inactive);
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    ChangeStatus(_status.Locked, false, _status.Inactive);
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }

        private void ChangeStatus(bool locked, bool sleeping, bool? inactive)
        {
            if (_status.Locked != locked ||
                _status.Sleeping != sleeping ||
                _status.Inactive != inactive)
            {
                _status.EndTime = DateTime.Now;
                Logging.Log(_status);
            }
            _status.StartTime = DateTime.Now;
            _status.Inactive = inactive;
            _status.Locked = locked;
            _status.Sleeping = sleeping;
        }

        private void Update()
        {
            var inactiveTime = SystemStats.GetInactiveTime();
            if(inactiveTime > Settings.Default.IdleTimeSeconds)
            {
                ChangeStatus(_status.Locked, _status.Sleeping, true);
            }
            else
            {
                if(_status.Inactive.Value)
                {
                    ChangeStatus(_status.Locked, _status.Sleeping, false);
                }
            }
            var energy = CorrectEnergyError(SystemStats.GetPowerUsageSinceLastQuery());
            _status.Value += (energy.joules + SystemStats.GetTotalMonitorWattUsage()) / 3600.0;
        }

        private EnergyInterval CorrectEnergyError(EnergyInterval energy)
        {
            if (energy.watts > _tdp * 10 || energy.joules > Settings.Default.TimerInterval * _tdp * 10)
            {
                energy.joules = _tdp * Settings.Default.TimerInterval;
                energy.watts = _tdp;
            }
            return energy;
        }

        private void SetupNextDBReport()
        {
            var now = DateTime.Now;
            var targetTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0); //Maybe settings
            if(targetTime < now)
            {
                targetTime = targetTime.AddDays(1);
            }
            var delay = targetTime - now; //forgot delay
            Task.Delay(delay).ContinueWith(t => { UpdateDatabase(); });
        }

        private List<Status> ReadStatuses()
        {
            var statuses = new List<Status>();
            if(File.Exists(Settings.Default.StatusCache))
            {
                var json = "[" + File.ReadAllText(Settings.Default.StatusCache) + "]";
                statuses = JsonConvert.DeserializeObject<List<Status>>(json);
            }            
            return statuses;
        }

        private void UpdateDatabase()
        {
            DBSettingsProvider.Update();
            var statuses = ReadStatuses();
            Settings.Default.GetUrl = DateTime.Now.ToString();
            Settings.Default.Save();
            DAL.UpdateStatuses(statuses);
            File.Delete(Settings.Default.StatusCache);
        }
    }
}
