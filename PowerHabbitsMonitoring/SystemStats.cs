using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerHabbitsMonitoring
{
    public class SystemStats
    {
        [DllImport("EnergyLib64.dll")]
        public static extern bool IntelEnergyLibInitialize();
        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetMsrName(int iMsr, StringBuilder szName);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetMsrFunc(int iMsr, ref int funcID);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetIAFrequency(int iNode, ref int freqInMHz);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetTDP(int iNode, ref double TDP);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetMaxTemperature(int iNode, ref int degreeC);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetTemperature(int iNode, ref int degreeC);
        [DllImport("EnergyLib64.dll")]
        public static extern bool ReadSample();
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetTimeInterval(ref double offset);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetBaseFrequency(int iNode, ref double baseFrequency);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetPowerData(int iNode, int iMSR, double[] result, ref int nResult);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetNumMsrs(ref int nMsr);
        [DllImport("EnergyLib64.dll")]
        public static extern bool GetNumNodes(ref int nNodes);

        private int _numMsrs = 0;
        private double _tdp = 18.0;
        private DBSettingsProvider _settings;

        public SystemStats(DBSettingsProvider settings)
        {
            try
            {
                _settings = settings;
                Environment.SetEnvironmentVariable("PATH", Settings.Default.DLLDirectory);
                if (IntelEnergyLibInitialize() == false)
                {
                    throw new Exception("Failed to init energy lib.");
                }
                GetNumMsrs(ref _numMsrs);
            }
            catch (Exception)
            {
            }

        }

        public double? GetInactiveTime()
        {
            try
            {
                var number = File.ReadAllText(Settings.Default.InactiveTimeFile);
                return double.Parse(number);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public double GetPowerUsageSinceLastQuery()
        {
            if (ReadSample() == false)
            {
                throw new Exception("Error reading new sample");
            }

            var whUsed = 0.0;
            for (int j = 0; j < _numMsrs; j++)
            {
                int funcID = 0;
                GetMsrFunc(j, ref funcID);
                double[] data = new double[3];
                int nData = 0;
                StringBuilder szName = new StringBuilder(260);

                GetPowerData(0, j, data, ref nData);
                GetMsrName(j, szName);

                if (szName.ToString() == "Processor")
                {
                    whUsed += data[2] / 1000.0;
                }

                if (szName.ToString() == "DRAM")
                {
                    whUsed += data[2] / 1000.0;
                }
            }

            return whUsed;
        }

        public double GetCorrectedTotalEnergy(double timePassed)
        {
            var cpuEnergy = GetPowerUsageSinceLastQuery();
            var maxCpuEnergy = _tdp * 10 * timePassed / 3600.0;
            if(cpuEnergy > maxCpuEnergy)
            {
                cpuEnergy = _tdp * timePassed / 3600.0;
            }
            var monitorEnergy = GetTotalMonitorWattUsage() * timePassed / 3600.0;
            return cpuEnergy + monitorEnergy;
        }

        public double GetTotalMonitorWattUsage()
        {
            var powerUsage = 0.0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "select * from WmiMonitorID");
            foreach (var o in searcher.Get())
            {
                foreach (var p in o.Properties)
                {
                    var str = "";
                    if (p.Name == "UserFriendlyName")
                    {
                        var arr = (ushort[])p.Value;
                        foreach (char c in arr)
                        {
                            if (char.IsLetterOrDigit(c) || c == ' ')
                            {
                                str += c;
                            }
                        }
                        if (_settings.Default.MonitorWattUsage.ContainsKey(str))
                        {
                            powerUsage += _settings.Default.MonitorWattUsage[str];
                        }
                        else
                        {
                            powerUsage += _settings.Default.DefaultMonitorWattUsage;
                        }
                    }
                }
            }
            return powerUsage;
        }
    }
}
