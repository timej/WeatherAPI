using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JmaXmlClient.Models
{
    public class ForecastSummary
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string AreaName { get; set; }
        [JsonProperty("sc")]
        public int StationCode { get; set; }
        [JsonProperty("sn")]
        public string StationName { get; set; }
        [JsonProperty("we")]
        public List<string> Weather { get; set; }
        [JsonProperty("wc")]
        public List<int> WeatherCode { get; set; }
        [JsonProperty("pre")]
        public List<int?> ProbabilityOfPrecipitation { get; set; }
        [JsonProperty("tp")]
        public List<JmaTemperature> JmaTemperatureList { get; set; }
    }

    public class PrefForecastSummary
    {
        [JsonProperty("pref")]
        public int Pref { get; set; }
        [JsonProperty("reported")]
        public DateTime ReportDateTime { get; set; }
        [JsonProperty("fs")]
        public List<ForecastSummary> ForecastSummaries { get; set; }
    }

    public class JmaForecastSummary
    {
        [JsonProperty("reported")]
        public DateTime ReportDateTime { get; set; }
        [JsonProperty("td")]
        public List<DateTime> TimeDefine { get; private set; }
        [JsonProperty("tdn")]
        public List<string> TimeDefineName { get; private set; }
        [JsonProperty("pretd")]
        public List<DateTime> PrecipitationTimeDefine { get; private set; }
        [JsonProperty("pfs")]
        public PrefForecastSummary[] PrefForecastSummaries { get; set; }

        public JmaForecastSummary()
        {
            SetIni();
        }

        public JmaForecastSummary(string json)
        {
            if (json == null)
                SetIni();
            else
            {
                var data = JsonConvert.DeserializeObject<JmaForecastSummary>(json);
                this.ReportDateTime = data.ReportDateTime;
                this.TimeDefine = data.TimeDefine;
                this.TimeDefineName = data.TimeDefineName;
                this.PrecipitationTimeDefine = data.PrecipitationTimeDefine;
                this.PrefForecastSummaries = data.PrefForecastSummaries;
            }
        }

        private void SetIni()
        {
            int[] prefList = { 11, 12, 13, 14, 15, 16, 17, 20, 50, 30, 60, 40, 70,
                80, 90, 100, 110, 120, 130, 140, 190, 200, 150, 160, 170, 180, 220, 230, 210, 240,
                250, 260, 270, 280, 290, 300, 330, 340, 320, 310, 360, 370, 380, 390,
                350, 400, 440, 420, 410, 430, 450, 460, 471, 472, 473, 474};
            PrefForecastSummaries = new PrefForecastSummary[prefList.Length];
            for (int n = 0; n < PrefForecastSummaries.Length; n++)
            {
                PrefForecastSummaries[n] = new PrefForecastSummary
                {
                    Pref = prefList[n],
                };
            }
        }

        public void ChangeJmaForecastSummary(List<JmaForecast> forcastList, DateTime forecastTime)
        {
            foreach (var forecast in forcastList)
            {
                //古いデータが来たときは無視する
                if (forecast.TimeDefine[0].ToUniversalTime() != forecastTime)
                    continue;

                if (forecast.ReportDateTime > this.ReportDateTime)
                    this.ReportDateTime = forecast.ReportDateTime;
                if(this.TimeDefine == null)
                {
                    this.TimeDefine = forecast.TimeDefine;
                    this.TimeDefineName = forecast.TimeDefineName;
                    this.PrecipitationTimeDefine = forecast.PrecipitationTimeDefine;
                }
                var prefForecast = PrefForecastSummaries.First(x => x.Pref == forecast.Prefecture);
                prefForecast.ReportDateTime = forecast.ReportDateTime;
                var forecastAreas = AppIni.ForecastForWeekly[forecast.Prefecture];
                if (prefForecast.ForecastSummaries == null)
                {
                    prefForecast.ForecastSummaries = new List<ForecastSummary>();
                    foreach (var forecastArea in forecastAreas)
                    {
                        var g = forecast.JmaForecastData.First(x => x.Id == forecastArea.area);
                        prefForecast.ForecastSummaries.Add(new ForecastSummary
                        {
                            Id = forecastArea.area,
                            AreaName = g.AreaName,
                            StationCode = g.StationCode,
                            StationName = g.StationName,
                            Weather = g.Weather,
                            WeatherCode = g.WeatherCode,
                            ProbabilityOfPrecipitation = g.ProbabilityOfPrecipitation,
                            JmaTemperatureList = g.JmaPointForecastList[0].JmaTemperatureList
                        });
                    }
                }
                else
                {
                    foreach (var forecastArea in forecastAreas)
                    {
                        var g = forecast.JmaForecastData.First(x => x.Id == forecastArea.area);
                        var f = prefForecast.ForecastSummaries.FirstOrDefault(x => x.Id == forecastArea.area);
                        if (f == null)
                        {
                            prefForecast.ForecastSummaries.Add(new ForecastSummary
                            {
                                Id = forecastArea.area,
                                AreaName = g.AreaName,
                                StationCode = g.StationCode,
                                StationName = g.StationName,
                                Weather = g.Weather,
                                WeatherCode = g.WeatherCode,
                                ProbabilityOfPrecipitation = g.ProbabilityOfPrecipitation,
                                JmaTemperatureList = g.JmaPointForecastList[0].JmaTemperatureList
                            });
                        }
                        else
                        {
                            f.Id = forecastArea.area;
                            f.AreaName = g.AreaName;
                            f.StationCode = g.StationCode;
                            f.StationName = g.StationName;
                            f.Weather = g.Weather;
                            f.WeatherCode = g.WeatherCode;
                            f.ProbabilityOfPrecipitation = g.ProbabilityOfPrecipitation;
                            f.JmaTemperatureList = g.JmaPointForecastList[0].JmaTemperatureList;
                        }
                    }
                }
            }
        }

        public static (DateTime dtUtc, int type ) GetForecastTime()
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var h = now.Hour;
            var type = 0;
            if(h == 1 || h == 7 || h == 19)
            {
                int m = now.Minute;
                if (m > 28)
                {
                    h++;
                    if (m < 46)
                        type = 1;
                    if (m < 58)
                        type = 2;
                    else
                        type = 3;
                }
            }
            //11時
            if (h < 2)
            {
                return (date.AddHours(-4), type);
            }
            //17時
            else if (h < 8)
                return (date.AddHours(2), type);
            //5時
            else if (h < 20)
                return (date.AddHours(8), type);
            else
            {
                return (date.AddHours(20), type);
            }
        }
    }
}