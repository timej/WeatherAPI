using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class WeeklyData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string AreaName { get; set; }
        [JsonProperty("we")]
        public List<string> Weather { get; set; }
        [JsonProperty("wc")]
        public List<int> WeatherCode { get; set; }
        [JsonProperty("pp")]
        public List<int?> ProbabilityOfPrecipitation { get; set; }
        [JsonProperty("rl")]
        public List<string> Reliability { get; set; }
        [JsonProperty("sc")]
        public int StationCode { get; set; }
        [JsonProperty("sn")]
        public string StationName { get; set; }
        [JsonProperty("lt")]
        public List<int?> LowestTeperature { get; set; }
        [JsonProperty("ltx")]
        public List<int?> LowestTeperatureMax { get; set; }
        [JsonProperty("ltn")]
        public List<int?> LowestTeperatureMinimum { get; set; }
        [JsonProperty("ht")]
        public List<int?> HighestTeperature { get; set; }
        [JsonProperty("htx")]
        public List<int?> HighestTeperatureMax { get; set; }
        [JsonProperty("htn")]
        public List<int?> HighestTeperatureMinimum { get; set; }

    }
}
