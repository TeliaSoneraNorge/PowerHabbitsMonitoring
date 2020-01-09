using System;
using System.IO;

namespace PowerHabbitsMonitoring
{
    public static class Logging
    {
        public static void Log(string line)
        {
            try
            {
                File.AppendAllText(Settings.Default.LogFileDirectory + $"/data{DateTime.Now.ToString("yyyy-MM-dd")}.csv", line);
            }
            catch (Exception)
            {
                //Ignore, might throw if file open in another process
                //If so it might be closed later.
            }
        }

        public static void LogEvents(string line)
        {
            try
            {
                File.AppendAllText(Settings.Default.LogFileDirectory + $"/events{DateTime.Now.ToString("yyyy-MM-dd")}.log", line + "\n");
            }
            catch (Exception)
            {
                //Ignore, might throw if file open in another process
                //If so it might be closed later.
            }
        }
    }
}
