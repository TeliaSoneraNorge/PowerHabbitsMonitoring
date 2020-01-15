using Microsoft.Win32;
using System;
using System.ServiceProcess;

namespace PowerHabbitsMonitoring
{
    public partial class PowerHabbitsMonitoring : ServiceBase
    {
        private System.Timers.Timer _timer;

        private double _joulesTotal = 0;
        private double _tdp = 18;

        private int _numMonitors = 3;
        private bool _locked = false;
        private bool _sleeping = false;

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
                    _numMonitors = 0;
                    Logging.LogEvents("Locked");
                    _locked = true;
                    break;

                case SessionChangeReason.SessionUnlock:
                    _numMonitors = SystemStats.GetNumMonitors();
                    Logging.LogEvents("Unlocked");
                    _locked = false;
                    break;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler((s, e) =>
            {
                switch (e.Mode)
                {
                    case PowerModes.Suspend:
                        break;

                    case PowerModes.Resume:
                        break;
                }
            });

            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    _numMonitors = 0;
                    Logging.LogEvents("Sleep");
                    _sleeping = true;
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    _numMonitors = SystemStats.GetNumMonitors();
                    Logging.LogEvents("Wake");
                    _sleeping = false;
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }

        private void Update()
        {
            _numMonitors = SystemStats.GetNumMonitors();
            var energy = CorrectEnergyError(SystemStats.GetPowerUsageSinceLastQuery());
            _joulesTotal += energy.joules + GetMonitorJoulesUsed(_numMonitors);
            WriteData(energy);
        }

        private void WriteData(EnergyInterval energy)
        {
            var line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}, " +
                       $"{(_locked ? "Locked" : "Unlocked")}, " +
                       $"{(_sleeping ? "Sleeping" : "Awake")}, " +
                       $"{Math.Round(_joulesTotal / 3600.0, 4)}, " +
                       $"{GetTotalWattUsage(energy, _numMonitors)}, " +
                       $"{_numMonitors}\n";

            Logging.Log(line);
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

        private double GetTotalWattUsage(EnergyInterval energy, int numMonitors)
        {
            return energy.watts + GetMonitorWattUsage(numMonitors);
        }

        private double GetMonitorWattUsage(int numMonitors)
        {
            return numMonitors * Settings.Default.PerMonitorUsage;
        }

        private double GetMonitorJoulesUsed(int numMonitors)
        {
            return GetMonitorWattUsage(numMonitors) * Settings.Default.TimerInterval;
        }
    }
}
