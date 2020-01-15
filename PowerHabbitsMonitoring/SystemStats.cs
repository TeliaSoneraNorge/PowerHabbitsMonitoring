using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerHabbitsMonitoring
{
    public class EnergyInterval
    {
        public double watts;
        public double joules;
    }

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
        public static extern bool StartLog(StringBuilder szFileName);

        [DllImport("EnergyLib64.dll")]
        public static extern bool StopLog();

        [DllImport("EnergyLib64.dll")]
        public static extern bool GetNumMsrs(ref int nMsr);

        [DllImport("EnergyLib64.dll")]
        public static extern bool GetNumNodes(ref int nNodes);

        private static int _numMsrs = 0;

        static SystemStats()
        {
            try
            {
                Environment.SetEnvironmentVariable("PATH", Settings.Default.DLLDirectory);
                if (IntelEnergyLibInitialize() == false)
                {
                    throw new Exception("Failed to init energy lib.");
                }
                GetNumMsrs(ref _numMsrs);
            }
            catch (Exception e)
            {
                Logging.Log(e.Message);
            }

        }

        public static double GetInactiveTime()
        {
            var inactiveTime = 0.0;
            try
            {
                var number = File.ReadAllText(Settings.Default.InactiveTimeFile);
                inactiveTime = double.Parse(number);
            }
            catch (Exception e)
            {
            }
            return inactiveTime;
        }

        public static EnergyInterval GetPowerUsageSinceLastQuery()
        {
            if (ReadSample() == false)
            {
                throw new Exception("Error reading new sample");
            }

            var JoulesUsed = 0.0;
            var WattsUsed = 0.0;
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
                    JoulesUsed += data[1];
                    WattsUsed += data[0];
                }

                if (szName.ToString() == "DRAM")
                {
                    JoulesUsed += data[1];
                    WattsUsed += data[0];
                }
            }

            return new EnergyInterval
            {
                watts = WattsUsed,
                joules = JoulesUsed
            };
        }

        public static int GetNumMonitors()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "select * from WmiMonitorBasicDisplayParams");
                return searcher.Get().Count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static double BatteryWhLeft()
        {
            ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection collection = searcher.Get();

            var whleft = 0.0;
            foreach (ManagementObject mo in collection)
            {
                whleft = int.Parse(mo["EstimatedChargeRemaining"].ToString()) * 56.0 / 100.0;
            }
            return whleft;
        }

        public static double GetTotalMonitorWattUsage()
        {
            return 0;
        }
    }
}
