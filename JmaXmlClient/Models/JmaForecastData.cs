using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class JmaForecastData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string AreaName { get; set; }
        [JsonProperty("wf")]
        public List<string> WeatherForecast { get; set; }
        [JsonProperty("we")]
        public List<string> Weather { get; set; }
        [JsonProperty("wc")]
        public List<int> WeatherCode { get; set; }
        [JsonProperty("wi")]
        public List<string> WindForecast { get; set; }
        [JsonProperty("wh")]
        public List<string> WaveHeight { get; set; }
        [JsonProperty("pre")]
        public List<int?> ProbabilityOfPrecipitation { get; set; }
        [JsonProperty("pf")]
        public List<JmaPointForecastData> JmaPointForecastList { get; set; }
        [JsonProperty("thwe")]
        public List<string> ThreeHourlyWeather { get; set; }
        [JsonProperty("thwd")]
        public List<string> WindDirection { get; set; }
        [JsonProperty("thws")]
        public List<int> WindSpeed { get; set; }
        [JsonProperty("sc")]
        public int StationCode { get; set; }
        [JsonProperty("sn")]
        public string StationName { get; set; }
        [JsonProperty("thte")]
        public List<int> ThreeHourlyTemperature { get; set; }
    }
}
