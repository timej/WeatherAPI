using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

//気象庁防災情報XMLの週間天気予報をJson形式に変換するプログラム

namespace JmaXmlClient.Models
{
    class Weekly : JmaForecastBase
    {
        [JsonProperty("td")]
        public List<DateTime> TimeDefine { get; set; }
        [JsonProperty("wd")]
        public List<WeeklyData> WeeklyDataList { get; set; }


        internal Weekly(string xml, int pref) : base(xml, pref)
        {
            try
            {
                var mis = xe.Descendants(Utils.XmlnsJmxEx + "MeteorologicalInfos");
                foreach (var m in mis)
                {
                    if (m.Attribute("type").Value == "区域予報")
                    {
                        WeeklyArea(m);
                    }
                    else if (m.Attribute("type").Value == "地点予報")
                    {
                        WeeklyPoint(m);
                        break;
                    }
                }
            }
            catch (Exception e1)
            {
                Utils.WriteLog($"Weeklyでエラー {e1.Message}").GetAwaiter().GetResult();
            }
        }

        public void WeeklyArea(XElement xe)
        {
            var ts0 = xe.Elements().First();

            var td = ts0.Element(Utils.XmlnsJmxEx + "TimeDefines");
            TimeDefine = new List<DateTime>();
            foreach (var t in td.Elements())
            {
                TimeDefine.Add(DateTime.Parse(t.Element(Utils.XmlnsJmxEx + "DateTime").Value));
            }

            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");
            WeeklyDataList = new List<WeeklyData>();
            foreach (var item in items)
            {
                var weeklyData = new WeeklyData {
                    Weather = new List<string>(),
                    WeatherCode = new List<int>(),
                    ProbabilityOfPrecipitation = new List<int?>(),
                    Reliability = new List<string>()
                };

                var wf = item.Descendants(Utils.XmlnsJmxEx + "WeatherPart");
                foreach (var w in wf.Elements())
                {
                    weeklyData.Weather.Add(w.Value);
                }

                var wc = item.Descendants(Utils.XmlnsJmxEx + "WeatherCodePart");
                foreach (var w in wc.Elements())
                {
                    weeklyData.WeatherCode.Add(int.Parse(w.Value));
                }

                var pp = item.Descendants(Utils.XmlnsJmxEx + "ProbabilityOfPrecipitationPart");
                foreach (var p in pp.Elements())
                {
                    weeklyData.ProbabilityOfPrecipitation.Add(p.Value == "" ? (int?)null :int.Parse(p.Value));
                }

                var rc = item.Descendants(Utils.XmlnsJmxEx + "ReliabilityClassPart");
                foreach (var p in rc.Elements())
                {
                    weeklyData.Reliability.Add(p.Value == "" ? null : p.Value);
                }

                var area = item.Element(Utils.XmlnsJmxEx + "Area");
                weeklyData.AreaName = area.Element(Utils.XmlnsJmxEx + "Name").Value;
                weeklyData.Id = int.Parse(area.Element(Utils.XmlnsJmxEx + "Code").Value);

                WeeklyDataList.Add(weeklyData);
            }
        }

        public void WeeklyPoint(XElement xe)
        {
            var ts0 = xe.Elements().First();

            var items = ts0.Elements(Utils.XmlnsJmxEx + "Item");
            int n = 0;
            foreach (var item in items)
            {
                var pointWeekly = WeeklyDataList[n];
                var tp = item.Descendants(Utils.XmlnsJmxEx + "TemperaturePart");
                foreach (var p in tp)
                {
                    List<int?> temp = new List<int?>();
                    foreach (var temperature in p.Elements())
                    {
                        temp.Add(temperature.Value == "" ? (int?)null : int.Parse(temperature.Value));
                    }

                    string s = p.Elements().First().Attribute("type").Value;
                    switch (s)
                    {
                        case "最低気温":
                            pointWeekly.LowestTeperature = temp;
                            break;
                        case "最低気温予測範囲（上端）":
                            pointWeekly.LowestTeperatureMax = temp;
                            break;
                        case "最低気温予測範囲（下端）":
                            pointWeekly.LowestTeperatureMinimum = temp;
                            break;
                        case "最高気温":
                            pointWeekly.HighestTeperature = temp;
                            break;
                        case "最高気温予測範囲（上端）":
                            pointWeekly.HighestTeperatureMax = temp;
                            break;
                        case "最高気温予測範囲（下端）":
                            pointWeekly.HighestTeperatureMinimum = temp;
                            break;
                    }
                }

                var station = item.Element(Utils.XmlnsJmxEx + "Station");
                pointWeekly.StationName = station.Element(Utils.XmlnsJmxEx + "Name").Value;
                pointWeekly.StationCode = int.Parse(station.Element(Utils.XmlnsJmxEx + "Code").Value);
                n++;
            }
        }

    }
}
