using Google.Cloud.Datastore.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using JmaXml.Common.Data;
using JmaXml.Common;

namespace JmaXmlClient.Models
{
    public class JmaDsRegularTask
    {
        public static async Task RegularAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("予報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                DateTime? update = await datastore.GetUpdateAsync("JmaXmlInfo", "JmaRegularFeeds2");
                var dt = Utils.GetForecastTime("vpfw50").AddMinutes(-30);
                if (update == null || update < dt)
                    update = dt;

                var list = await datastore.GetJmaFeed("regular", (DateTime)update);
                if (!list.Any())
                    return;
                DateTime lastUpdate = list.First().Properties["created"].TimestampValue.ToDateTime();

                foreach (var xmlRegular in list)
                {
                    string s = xmlRegular.Properties["feeds"].StringValue;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(s);
                    foreach (var feed in feeds)
                    {
                        Utils.AddFeed(feedList, feed);
                    }
                }

                //電文を受け取れなかった場合、PubSubHubbubは再送をしてくれる。
                //その場合受けとる順番はランダムになるため、新しい電文を古い電文で置き換えないかチェック
                List<(int id, long update)?>[] updateList = new List<(int id, long update)?>[Enum.GetNames(typeof(JmaForecastTask)).Length];
                foreach (JmaForecastTask task in Enum.GetValues(typeof(JmaForecastTask)))
                {
                    if(feedList.Any(x => x.Task == task.ToString()))
                    {
                        updateList[(int)task] = await datastore.GetJmaUpdateAsync(task.ToString());
                    }
                }

                var feedList1 = new List<JmaFeedData2>();
                foreach(var feed in feedList)
                {
                    var previous = updateList[(int)Enum.Parse(typeof(JmaForecastTask), feed.Task)].FirstOrDefault(x => x?.id == feed.Id);
                    long l = Utils.UnixTime(feed.UpdateTime);
                    if (previous == null || previous?.update < l)
                        feedList1.Add(feed);
                }
                await UpsertData(feedList1);
                await datastore.SetUpdateAsync("JmaXmlInfo", "JmaRegularFeeds2", lastUpdate);
               

                await Utils.WriteLog("予報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }


        //天気予報等の処理-Datastoreを使用
        static async Task UpsertData(List<JmaFeedData2> forecastList)
        {
            var datastore1 = new JmaDatastore(AppIni.ProjectId);
            var datastore2 = new JmaDatastore(AppIni.ProjectId);
            var entityList1 = new List<Entity>();
            var entityList2 = new List<Entity>();
            var vpfd50List = new List<JmaForecast>();
            var vpfw50List = new List<Weekly>();

            foreach (var forecast in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(forecast.Link);
                entityList1.Add(datastore1.SetEntity("JmaXml", forecast.Task, forecast.Id, xml, forecast.UpdateTime.ToUniversalTime()));
                string json;
                if(forecast.Task == "vpfd50")
                    json = JsonVpfd50(xml, forecast.Id, vpfd50List);
                else if(forecast.Task == "vpfw50")
                    json = JsonVpfw50(xml, forecast.Id, vpfw50List);
                else
                    json = JsonCondition(xml, forecast.Id);
                entityList2.Add(datastore2.SetEntity("JmaJson", forecast.Task, forecast.Id, json, forecast.UpdateTime.ToUniversalTime()));
            }

            await datastore1.UpsertForecastAsync(entityList1);
            await datastore2.UpsertForecastAsync(entityList2);

            if(vpfd50List.Any())
            {
                await SetSummary(vpfd50List);
            }

            if (vpfw50List.Any())
            {
                await SetWeeklySummary(vpfw50List);
            }
        }


        static string JsonVpfd50(string xml, int id, List<JmaForecast> jmaForecastList)
        {
            JmaForecast jmaForecast = new JmaForecast(xml, id);
            jmaForecastList.Add(jmaForecast);
            return JsonConvert.SerializeObject(jmaForecast);
        }

        static string JsonVpfw50(string xml, int id, List<Weekly> weeklyList)
        {
            Weekly weekly = new Weekly(xml, id);
            weeklyList.Add(weekly);
            return JsonConvert.SerializeObject(weekly);
        }

        static string JsonCondition(string xml, int id)
        {
            var conditions = new WeatherConditions(xml, id);
            return JsonConvert.SerializeObject(conditions);
        }

        internal static async Task SetSummary(List<JmaForecast> forecastList)
        {
            var (date, type) = JmaForecastSummary.GetForecastTime();
            if (type == 0)
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                string json = await datastore.GetInfoDataAsync("forecastSummaries");
                var summary = new JmaForecastSummary(json);
                summary.ChangeJmaForecastSummary(forecastList, date);
                await datastore.SetInfoDataAsnc("forecastSummaries", JsonConvert.SerializeObject(summary), DateTime.UtcNow);
            }
            else
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                string json = await datastore.GetInfoDataAsync("forecastSummaries" + date.ToString("yyyyMMddTHH"));
                var summary = new JmaForecastSummary(json);
                summary.ChangeJmaForecastSummary(forecastList, date);
                string json2 = JsonConvert.SerializeObject(summary);
                await datastore.SetInfoDataAsnc("forecastSummaries" + date.ToString("yyyyMMddTHH"), json2, DateTime.UtcNow);
                if ((type == 2 && !summary.PrefForecastSummaries.Any(x => x.ReportDateTime == default(DateTime))) || type == 3)
                {
                    await datastore.SetInfoDataAsnc("forecastSummaries", json2, DateTime.UtcNow);
                }
            }
        }

        internal static async Task SetWeeklySummary(List<Weekly> weeklyList)
        {
            var (date, type) = WeeklySummary.GetForecastTime();
            if (type == 0)
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                string json = await datastore.GetInfoDataAsync("weeklySummaries");
                var summary = new WeeklySummary(json);
                summary.ChangeWeeklySummary(weeklyList, date);
                await datastore.SetInfoDataAsnc("weeklySummaries", JsonConvert.SerializeObject(summary), DateTime.UtcNow);
            }
            else
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                string json = await datastore.GetInfoDataAsync("weeklySummaries" + date.ToString("yyyyMMddTHH"));
                var summary = new WeeklySummary(json);
                summary.ChangeWeeklySummary(weeklyList, date);
                string json2 = JsonConvert.SerializeObject(summary);
                await datastore.SetInfoDataAsnc("weeklySummaries" + date.ToString("yyyyMMddTHH"), json2, DateTime.UtcNow);
                if ((type == 2 && !summary.PrefWeeklySummaries.Any(x => x.ReportDateTime == default(DateTime))) || type == 3)
                {
                    await datastore.SetInfoDataAsnc("weeklySummaries", json2, DateTime.UtcNow);
                }
            }
        }
    }
}
