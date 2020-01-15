using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PowerHabbitsMonitoring
{
    public static class DAL
    {
        public static void UpdateStatuses(List<Status> statuses)
        {
            using(var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(statuses);
                var rez = client.PostAsync(Settings.Default.PostUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            }
        }
    }
}
