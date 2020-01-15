using Microsoft.Win32;
using System;
using System.ServiceProcess;

namespace PowerHabbitsMonitoring
{
    public partial class PowerHabbitsMonitoring : ServiceBase
    {
        private System.Timers.Timer _timer;
        private double _tdp = 18;
        private Status _status = new Status();

        public PowerHabbitsMonitoring()
        {
            InitializeComponent();
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
                _timer = new System.Timers.Timer(Settings.Default.TimerInterval * 1000);
                Update();
                _timer.Elapsed += (s, e) =>
                {
                    Update();
                };
                _timer.Start();
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
    }
}
