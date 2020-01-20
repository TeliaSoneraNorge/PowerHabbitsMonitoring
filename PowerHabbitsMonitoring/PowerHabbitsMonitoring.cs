using System.ServiceProcess;

namespace PowerHabbitsMonitoring
{
    public partial class PowerHabbitsMonitoring : ServiceBase
    {
        private DBService _dbService;
        private StatusService _statusService;
        private DBSettingsProvider _settings;


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
            _settings = new DBSettingsProvider();
            _statusService = new StatusService(_settings);
            _dbService = new DBService(_settings);

            _statusService.Run();
            _dbService.Run();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLock:
                    _statusService.LockUnlock(true);
                    _dbService.SendIfNeedsSending();
                    break;

                case SessionChangeReason.SessionUnlock:
                    _statusService.LockUnlock(false);
                    _dbService.SendIfNeedsSending();
                    break;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    _statusService.SleepingAwake(true);
                    _dbService.SendIfNeedsSending();
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    _statusService.SleepingAwake(false);
                    _dbService.SendIfNeedsSending();
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }
    }
}
