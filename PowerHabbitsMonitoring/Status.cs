using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerHabbitsMonitoring
{
    public class Status
    {
        public string MachineName { get; set; } = Environment.MachineName;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
        public bool Locked { get; set; } = false;
        public bool Sleeping { get; set; } = false;
        public bool? Inactive { get; set; } = false;
        public double Value { get; set; } = 0;
    }
}
