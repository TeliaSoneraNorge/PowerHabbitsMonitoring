using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace PowerHabbitsMonitoring
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.serviceInstaller1.Description = "Tracks the energy that user uses.";
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalService;
        }
    }
}
