using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    public class Utils
    {
        public static readonly XNamespace XmlnsJmx = "http://xml.kishou.go.jp/jmaxml1/";
        public static readonly XNamespace XmlnsJmxIb = "http://xml.kishou.go.jp/jmaxml1/informationBasis1/";
        public static readonly XNamespace XmlnsJmxEx = "http://xml.kishou.go.jp/jmaxml1/body/meteorology1/";
        public static readonly XNamespace XmlnsJmxEb = "http://xml.kishou.go.jp/jmaxml1/elementBasis1/";

        public static async Task WriteLog(string message)
        {
            string path = Path.Combine(AppIni.DataPath, "logs", $"log-{DateTime.Today.ToString("yyyyMMdd")}.txt");
            using (var sr = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write)))
            {
                await sr.WriteLineAsync($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")} {message}");
            }
        }

        internal static void AddFeed(List<JmaFeedData> forecastList, JmaXmlFeed feed)
        {
            int id = AppIni.PublishingOffice[feed.Author];
            var forecast = forecastList.FirstOrDefault(x => x.Id == id);
            if (forecast == null)
                forecastList.Add(new JmaFeedData
                {
                    Id = id,
                    UpdateTime = feed.UpdateTime,
                    Link = feed.Link
                });
            else
            {
                if (forecast.UpdateTime < feed.UpdateTime)
                {
                    forecast.UpdateTime = feed.UpdateTime;
                    forecast.Link = feed.Link;
                }
            }
        }

        internal static void AddFeed(List<JmaFeedData2> forecastList, JmaXmlFeed feed)
        {
            int id = AppIni.PublishingOffice[feed.Author];
            var forecast = forecastList.FirstOrDefault(x => x.Id == id && x.Task == feed.Task);
            if (forecast == null)
                forecastList.Add(new JmaFeedData2
                {
                    Id = id,
                    Task = feed.Task,
                    UpdateTime = feed.UpdateTime,
                    Link = feed.Link
                });
            else
            {
                if (forecast.UpdateTime < feed.UpdateTime)
                {
                    forecast.UpdateTime = feed.UpdateTime;
                    forecast.Link = feed.Link;
                }
            }
        }

        public static DateTime GetForecastTime(string code)
        {
            var now = DateTime.UtcNow;
            //11時
            if (now.Hour < 2)
            {
                now = now.AddDays(-1);
                if (code == "vpfd50")
                    return new DateTime(now.Year, now.Month, now.Day, 20, 0, 0, DateTimeKind.Utc);
                else
                    return new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc);
            }
            //17時
            else if (now.Hour < 8)
                return new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc);
            //5時
            else if (now.Hour < 20)
                return new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc);
            else
            {
                if (code == "vpfd50")
                    return new DateTime(now.Year, now.Month, now.Day, 20, 0, 0, DateTimeKind.Utc);
                else
                    return new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc);
            }
        }
    }
}
