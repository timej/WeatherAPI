using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    public class JmaPointForecastData
    {
        [JsonProperty("sc")]
        public int StationCode { get; set; }
        [JsonProperty("sn")]
        public string StationName { get; set; }
        [JsonProperty("tp")]
        public List<JmaTemperature> JmaTemperatureList { get; set; }
    }
}
