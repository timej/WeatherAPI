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

            foreach (var forecast in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(forecast.Link);
                entityList1.Add(datastore1.SetEntity("JmaXml", forecast.Task, forecast.Id, xml, forecast.UpdateTime.ToUniversalTime()));
                string json;
                if(forecast.Task == "vpfd50")
                    json = JsonVpfd50(xml, forecast.Id);
                else if(forecast.Task == "vpfw50")
                    json = JsonVpfw50(xml, forecast.Id);
                else
                    json = JsonCondition(xml, forecast.Id);
                entityList2.Add(datastore2.SetEntity("JmaJson", forecast.Task, forecast.Id, json, forecast.UpdateTime.ToUniversalTime()));
            }

            await datastore1.UpsertForecastAsync(entityList1);
            await datastore2.UpsertForecastAsync(entityList2);
        }


        static string JsonVpfd50(string xml, int id)
        {
            JmaForecast jmaForecast = new JmaForecast(xml, id);
            return JsonConvert.SerializeObject(jmaForecast);
        }

        static string JsonVpfw50(string xml, int id)
        {
            Weekly weekly = new Weekly(xml, id);
            return JsonConvert.SerializeObject(weekly);
        }

        static string JsonCondition(string xml, int id)
        {
            var conditions = new WeatherConditions(xml, id);
            return JsonConvert.SerializeObject(conditions);
        }
    }
}
