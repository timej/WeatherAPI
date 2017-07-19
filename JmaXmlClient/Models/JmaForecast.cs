using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JmaXml.Common;

//気象庁防災情報XMLの天気予報をJson形式に変換するプログラム

namespace JmaXmlClient.Models
{
    public class JmaForecast: JmaForecastBase
    {
        [JsonProperty("td")]
        public List<DateTime> TimeDefine { get; private set; }
        [JsonProperty("tdn")]
        public List<string> TimeDefineName { get; private set; }
        [JsonProperty("pretd")]
        public List<DateTime> PrecipitationTimeDefine { get; private set; }
        [JsonProperty("pretdn")]
        public List<string> PrecipitationTimeDefineName { get; private set; }
        [JsonProperty("threetd")]
        public List<DateTime> ThreeHourlyTimeDefine { get; private set; }
        [JsonProperty("forecast")]
        public List<JmaForecastData> JmaForecastData { get; private set; }

        private List<JmaAreaForecastData> JmaAreaForecastDataList;
        private List<JmaPointForecastData> PointForecastList;
        private List<ThreeHourlyAreaData> ThreeHourlyAreaDataList;
        private List<ThreeHourlyPointData> ThreeHourlyPointDataList;

        public JmaForecast():base ()
        { }
        public JmaForecast(string xml, int pref) : base (xml, pref)
        {
            try
            {
                var mi = xe.Descendants(Utils.XmlnsJmxEx + "MeteorologicalInfos");
                int narea = 0;
                int npoint = 0;
                foreach (var m in mi)
                {
                    if (m.Attribute("type").Value == "区域予報")
                    {
                        if (narea == 0)
                            ForecastArea(m);
                        else if (npoint == 1)
                            ThreeHourlyArea(m);
                        narea++;
                    }
                    else if (m.Attribute("type").Value == "地点予報")
                    {
                        if (npoint == 0)
                            ForecastPoint(m);
                        else if (npoint == 1)
                            ThreeHourlyPoint(m);
                        npoint++;
                    }
                }

                JmaForecastData = new List<JmaForecastData>();
                foreach (var data in JmaAreaForecastDataList)
                {
                    JmaForecastData.Add(new JmaForecastData
                    {
                        Id = data.Id,
                        AreaName = data.AreaName,
                        WeatherForecast = data.WeatherForecast,
                        Weather = data.Weather,
                        WeatherCode = data.WeatherCode,
                        WindForecast = data.WindForecast,
                        WaveHeight = data.WaveHeight,
                        ProbabilityOfPrecipitation = data.ProbabilityOfPrecipitation,
                        JmaPointForecastList = new List<JmaPointForecastData> {
                            null
                        }
                    });
                }

                foreach (var forecast in PointForecastList)
                {

                    int num = ThreeHourlyPointDataList.FindIndex(x => x.StationCode == forecast.StationCode);
                    if (num > -1)
                    {
                        JmaForecastData[num].JmaPointForecastList[0] = forecast;
                    }
                    else
                    {
                        var forecastArea = JmaForecastArea.ForecastAreaOfStations;
                        var forecastAreaData = forecastArea.FirstOrDefault(x => x.観測所コード == forecast.StationCode);

                        if (forecastAreaData == null)
                        {
                            forecastAreaData = forecastArea.FirstOrDefault(x => x.地点 == forecast.StationName && x.気象区コード / 1000 == pref);
                            if (forecastAreaData == null)
                            {
                                Utils.WriteLog($"観測所コード: {forecast.StationCode} 地点名: {forecast.StationName} は、観測所気象区コード表にありません。").Wait();
                                continue;
                            }
                            else
                                Utils.WriteLog($"観測所気象区コード表で、地点名: {forecast.StationName} の観測所コード: {forecast.StationCode} に変更があったようです。").Wait();
                        }
                           
                        int code = (forecastAreaData.気象区コード / 10 == 1101) ? 11000 : forecastAreaData.気象区コード / 10 * 10;
                        var forecastData = JmaForecastData.First(x => x.Id == code);
                        forecastData.JmaPointForecastList.Add(forecast);
                    }
                }

                int n = 0;
                foreach (var threeHourly in ThreeHourlyAreaDataList)
                {
                    JmaForecastData[n].ThreeHourlyWeather = threeHourly.Weather;
                    JmaForecastData[n].WindDirection = threeHourly.WindDirection;
                    JmaForecastData[n].WindSpeed = threeHourly.WindSpeed;
                    n++;
                }
                n = 0;
                foreach (var threeHourly in ThreeHourlyPointDataList)
                {
                    JmaForecastData[n].ThreeHourlyTemperature = threeHourly.Temperature;
                    JmaForecastData[n].StationCode = threeHourly.StationCode;
                    JmaForecastData[n].StationName = threeHourly.StationName;
                    n++;
                }
            }
            catch(Exception e1)
            {
                Utils.WriteLog($"JmaForecastでエラー {pref} {e1.Message}").GetAwaiter().GetResult();
            }
        }

        public void ForecastArea(XElement xe)
        {
            var ts0 = xe.Elements().First();

            var td = ts0.Element(Utils.XmlnsJmxEx + "TimeDefines");
            TimeDefine = new List<DateTime>();
            TimeDefineName = new List<string>();
            foreach (var t in td.Elements())
            {
                TimeDefine.Add(DateTime.Parse(t.Element(Utils.XmlnsJmxEx + "DateTime").Value));
                TimeDefineName.Add(t.Element(Utils.XmlnsJmxEx + "Name").Value);
            }

            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");
            JmaAreaForecastDataList = new List<JmaAreaForecastData>();
            foreach (var item in items)
            {
                var jmaForecastData = new JmaAreaForecastData()
                {
                    WeatherForecast = new List<string>()
                };
                var wf = item.Descendants(Utils.XmlnsJmxEx + "WeatherForecastPart");
                foreach (var w in wf)
                {
                    jmaForecastData.WeatherForecast.Add(w.Element(Utils.XmlnsJmxEx + "Sentence").Value);
                }

                var wp = item.Descendants(Utils.XmlnsJmxEx + "WeatherPart").First();
                jmaForecastData.Weather = new List<string>();
                foreach (var w in wp.Elements())
                {
                    jmaForecastData.Weather.Add(w.Value);
                }

                jmaForecastData.WeatherCode = new List<int>();
                var wc = item.Descendants(Utils.XmlnsJmxEb + "WeatherCode");
                foreach (var w in wc)
                {
                    jmaForecastData.WeatherCode.Add(int.Parse(w.Value));
                }

                jmaForecastData.WindForecast = new List<string>();
                var wd = item.Descendants(Utils.XmlnsJmxEx + "WindForecastPart");
                foreach (var w in wd)
                {
                    jmaForecastData.WindForecast.Add(w.Element(Utils.XmlnsJmxEx + "Sentence").Value);
                }

                jmaForecastData.WaveHeight = new List<string>();
                var wh = item.Descendants(Utils.XmlnsJmxEx + "WaveHeightForecastPart");
                foreach (var w in wh)
                {
                    jmaForecastData.WaveHeight.Add(w.Element(Utils.XmlnsJmxEx + "Sentence").Value);
                }

                var area = item.Element(Utils.XmlnsJmxEx + "Area");
                jmaForecastData.AreaName = area.Element(Utils.XmlnsJmxEx + "Name").Value;
                jmaForecastData.Id = int.Parse(area.Element(Utils.XmlnsJmxEx + "Code").Value);

                JmaAreaForecastDataList.Add(jmaForecastData);
            }

            //降水確率の取得
            //TimeSeriesInfo
            var ts1 = xe.Elements().Skip(1).First();

            var pt = ts1.Element(Utils.XmlnsJmxEx + "TimeDefines");
            PrecipitationTimeDefine = new List<DateTime>();
            PrecipitationTimeDefineName = new List<string>();
            foreach (var t in pt.Elements())
            {
                PrecipitationTimeDefine.Add(DateTime.Parse(t.Element(Utils.XmlnsJmxEx + "DateTime").Value));
                PrecipitationTimeDefineName.Add(t.Element(Utils.XmlnsJmxEx + "Name").Value);
            }

            var pps = ts1.Descendants(Utils.XmlnsJmxEx + "Item");

            int n = 0;
            foreach(var item in pps)
            {
                var pp = item.Descendants(Utils.XmlnsJmxEb + "ProbabilityOfPrecipitation");
                JmaAreaForecastDataList[n].ProbabilityOfPrecipitation = new List<int?>();
                foreach (var w in pp)
                {
                    if (int.TryParse(w.Value, out int x))
                        JmaAreaForecastDataList[n].ProbabilityOfPrecipitation.Add(x);
                    else
                        JmaAreaForecastDataList[n].ProbabilityOfPrecipitation.Add(null);
                }
                n++;
            }
        }

        private void ForecastPoint(XElement xe)
        {
            var ts0 = xe.Elements().First();

            var td = ts0.Element(Utils.XmlnsJmxEx + "TimeDefines");
            var pointTimeDefine = new List<int>();
            foreach (var t in td.Elements())
            {
                string s = t.Element(Utils.XmlnsJmxEx + "Name").Value;
                if (s.StartsWith("今日"))
                    pointTimeDefine.Add(0);
                if (s.StartsWith("明日"))
                    pointTimeDefine.Add(1);
                if (s.StartsWith("明後日"))
                    pointTimeDefine.Add(2);
            }

            PointForecastList = new List<JmaPointForecastData>();
            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");

            foreach(var item in items)
            {
                JmaPointForecastData pointForecast = new JmaPointForecastData {
                    JmaTemperatureList = new List<JmaTemperature>()
                };
                for (int d = 0; d < TimeDefine.Count; d++)
                    pointForecast.JmaTemperatureList.Add(new JmaTemperature());

                int n = 0;
                var temperatures = item.Descendants(Utils.XmlnsJmxEb + "Temperature");
                foreach(var temperature in temperatures)
                {
                    string type = temperature.Attribute("type").Value;
                    if (type == "日中の最高気温")
                        pointForecast.JmaTemperatureList[pointTimeDefine[n]].DayTimeMax = int.Parse(temperature.Value);
                    if (type == "最高気温")
                        pointForecast.JmaTemperatureList[pointTimeDefine[n]].Max = int.Parse(temperature.Value);
                    if (type == "朝の最低気温")
                        pointForecast.JmaTemperatureList[pointTimeDefine[n]].MorningMin = int.Parse(temperature.Value);
                    if (type == "最低気温")
                        pointForecast.JmaTemperatureList[pointTimeDefine[n]].Min = int.Parse(temperature.Value);
                    n++;
                }

                var station = item.Element(Utils.XmlnsJmxEx + "Station");
                pointForecast.StationName = station.Element(Utils.XmlnsJmxEx + "Name").Value;
                pointForecast.StationCode = int.Parse(station.Element(Utils.XmlnsJmxEx + "Code").Value);

                PointForecastList.Add(pointForecast);
            }
        }

        private void ThreeHourlyArea(XElement xe)
        {
            var ts0 = xe.Elements().First();
            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");
            ThreeHourlyAreaDataList = new List<ThreeHourlyAreaData>();
            foreach (var item in items)
            {
                var threeHourlyData = new ThreeHourlyAreaData()
                {
                    Weather = new List<string>()
                };
                var wf = item.Descendants(Utils.XmlnsJmxEx + "WeatherPart");
                foreach (var w in wf.Elements())
                {
                    threeHourlyData.Weather.Add(w.Value);
                }

                var wd = item.Descendants(Utils.XmlnsJmxEx + "WindDirectionPart").First();
                threeHourlyData.WindDirection = new List<string>();
                foreach (var w in wd.Elements())
                {
                    threeHourlyData.WindDirection.Add(w.Value);
                }

                var ws = item.Descendants(Utils.XmlnsJmxEx + "WindSpeedPart").First();
                threeHourlyData.WindSpeed = new List<int>();
                foreach (var w in ws.Elements())
                {
                    threeHourlyData.WindSpeed.Add(int.Parse(w.Value));
                }
                ThreeHourlyAreaDataList.Add(threeHourlyData);
            }
        }

        private void ThreeHourlyPoint(XElement xe)
        {
            var ts0 = xe.Elements().First();

            var td = ts0.Element(Utils.XmlnsJmxEx + "TimeDefines");
            ThreeHourlyTimeDefine = new List<DateTime>();
            foreach (var t in td.Elements())
            {
                ThreeHourlyTimeDefine.Add(DateTime.Parse(t.Element(Utils.XmlnsJmxEx + "DateTime").Value));
            }

            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");
            ThreeHourlyPointDataList = new List<ThreeHourlyPointData>();
            foreach (var item in items)
            {
                var threeHourlyData = new ThreeHourlyPointData()
                {
                    Temperature = new List<int>()
                };
                var wf = item.Descendants(Utils.XmlnsJmxEx + "TemperaturePart");
                foreach (var w in wf.Elements())
                {
                    threeHourlyData.Temperature.Add(int.Parse(w.Value));
                }

                var station = item.Element(Utils.XmlnsJmxEx + "Station");
                threeHourlyData.StationName = station.Element(Utils.XmlnsJmxEx + "Name").Value;
                threeHourlyData.StationCode = int.Parse(station.Element(Utils.XmlnsJmxEx + "Code").Value);
                ThreeHourlyPointDataList.Add(threeHourlyData);
            }
        }
    }
}
