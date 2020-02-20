using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private double? inactiveTime = 0;

        public SystemStats(DBSettingsProvider settings)
        {
            try
            {
                _settings = settings;
                Environment.SetEnvironmentVariable("PATH", AppDomain.CurrentDomain.BaseDirectory);
                if (IntelEnergyLibInitialize() == false)
                {
                    throw new Exception("Failed to init energy lib.");
                }
                GetNumMsrs(ref _numMsrs);
                StartGettingInactiveTime();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception on creating static SystemStats instance.");
            }
        }

        public void StartGettingInactiveTime()
        {
            Task.Run(() =>
            {
                while (RunNamedPipe() == 1)
                {
                    inactiveTime = null;
                }
            });
        }

        private int RunNamedPipe()
        {
            var exitCode = 0;
            using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream("LastActivityTracker"))
            {
                namedPipeClient.Connect();
                using (var reader = new BinaryReader(namedPipeClient))
                {
                    while (exitCode == 0)
                    {
                        try
                        {
                            inactiveTime = reader.ReadDouble();
                        }
                        catch (Exception e)
                        {
                            exitCode = 1;
                            _logger.Error("Named pipe 'LastActivityTrackerClosed unexpectedly'");
                        }
                    }

                }
            }
            return exitCode;
        }


        public double? GetInactiveTime()
        {
            return inactiveTime;
        }

        public double GetPowerUsageSinceLastQuery()
        {
            try
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
            catch (Exception e)
            {
                _logger.Error(e, "Exception getting power.");
                return 0;
            }         
        }

        public double GetCorrectedTotalEnergy(double timePassed)
        {
            try
            {
                var cpuEnergy = GetPowerUsageSinceLastQuery();
                var maxCpuEnergy = _tdp * 10 * timePassed / 3600.0;
                if (cpuEnergy > maxCpuEnergy)
                {
                    cpuEnergy = _tdp * timePassed / 3600.0;
                }
                var monitorEnergy = GetTotalMonitorWattUsage() * timePassed / 3600.0;
                return cpuEnergy + monitorEnergy;
            }
            catch(Exception e)
            {
                _logger.Error(e, "Exception getting total energy.");
                return 0;
            }
        }

        public double GetTotalMonitorWattUsage()
        {
            try
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
            catch(Exception e)
            {
                _logger.Error(e, "Error getting monitor watts.");
                return 0;
            }            
        }
    }
}
