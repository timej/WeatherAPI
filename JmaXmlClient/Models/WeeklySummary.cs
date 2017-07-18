using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JmaXmlClient.Models
{
    class PrefWeeklySummary
    {
        [JsonProperty("pref")]
        public int Pref { get; set; }
        [JsonProperty("reported")]
        public DateTime ReportDateTime { get; set; }
        [JsonProperty("data")]
        public List<WeeklyData> WeeklyDataList { get; set; }
    }

    class WeeklySummary
    {
        [JsonProperty("reported")]
        public DateTime ReportDateTime { get; set; }
        [JsonProperty("td")]
        public List<DateTime> TimeDefine { get; set; }
        [JsonProperty("pws")]
        public PrefWeeklySummary[] PrefWeeklySummaries { get; set; }


        public WeeklySummary()
        {
            SetIni();
        }

        public WeeklySummary(string json)
        {
            if (json == null)
                SetIni();
            else
            {
                var data = JsonConvert.DeserializeObject<WeeklySummary>(json);
                this.ReportDateTime = data.ReportDateTime;
                this.TimeDefine = data.TimeDefine;
                this.PrefWeeklySummaries = data.PrefWeeklySummaries;
            }
        }

        private void SetIni()
        {
            int[] prefList = { 11, 12, 13, 14, 15, 16, 17, 20, 50, 30, 60, 40, 70,
                80, 90, 100, 110, 120, 130, 140, 190, 200, 150, 160, 170, 180, 220, 230, 210, 240,
                250, 260, 270, 280, 290, 300, 330, 340, 320, 310, 360, 370, 380, 390,
                350, 400, 440, 420, 410, 430, 450, 460, 471, 472, 473, 474};
            PrefWeeklySummaries = new PrefWeeklySummary[prefList.Length];
            for (int n = 0; n < PrefWeeklySummaries.Length; n++)
            {
                PrefWeeklySummaries[n] = new PrefWeeklySummary
                {
                    Pref = prefList[n]
                };
            }
        }

        public void ChangeWeeklySummary(List<Weekly> weeklyList, DateTime forecastTime)
        {
            foreach (var weekly in weeklyList)
            {
                //古いデータが来たときは無視する
                if (weekly.ReportDateTime.ToUniversalTime() < forecastTime)
                    continue;

                if (weekly.ReportDateTime > this.ReportDateTime)
                    this.ReportDateTime = weekly.ReportDateTime;
                if (this.TimeDefine == null)
                    this.TimeDefine = weekly.TimeDefine;

                var prefWeekly = PrefWeeklySummaries.First(x => x.Pref == weekly.Prefecture);
                prefWeekly.ReportDateTime = weekly.ReportDateTime;
                prefWeekly.WeeklyDataList = weekly.WeeklyDataList;
            }
        }

        public static (DateTime dtUtc, int type) GetForecastTime()
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var h = now.Hour;
            var type = 0;
            if (h == 1 || h == 7 || h == 19)
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
                return (date.AddHours(-16), type);
            //17時
            else if (h < 8)
                return (date.AddHours(2), type);
            else
                return (date.AddHours(8), type);
        }
    }
}