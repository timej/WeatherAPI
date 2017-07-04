using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class WeeklyList
    {
        public DateTime ReportDateTime { get; set; }
        public List<LocalWeekly> LocalWeeklyForecasts { get; set; }
    }

    public class LocalWeekly
    {
        public string AreaCode { get; set; }
        public DateTime ReportDateTime { get; set; }
        public List<LocalWeeklyData> WeeklyForecasts { get; set; }
    }

    public class LocalWeeklyData
    {
        public string 区域 { get; set; }
        public string 区域コード { get; set; }
        public string 地点 { get; set; }
        public string 地点コード { get; set; }
        public string[] 天気 { get; set; }
        public string[] WeatherCode { get; set; }
        public int?[] 降水確率 { get; set; }
        public string[] 信頼度 { get; set; }
        public int?[] 最低気温 { get; set; }
        public int?[] 最低気温上端 { get; set; }
        public int?[] 最低気温下端 { get; set; }
        public int?[] 最高気温 { get; set; }
        public int?[] 最高気温上端 { get; set; }
        public int?[] 最高気温下端 { get; set; }
    }
}
