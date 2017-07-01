using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class JmaAreaForecastData
    {
        public int Id { get; set; }
        public string AreaName { get; set; }
        public List<string> WeatherForecast { get; set; }
        public List<string> Weather { get; set; }
        public List<int> WeatherCode { get; set; }
        public List<string> WindForecast { get; set; }
        public List<string> WaveHeight { get; set; }
        public List<int?> ProbabilityOfPrecipitation { get; set; }
    }
}
