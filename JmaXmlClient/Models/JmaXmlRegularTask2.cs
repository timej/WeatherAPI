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
    enum JmaForecastTask { vpfd50, vpfg50, vpfw50, vpcw50, vpzw50};
    public class JmaXmlRegularTask2
    {
        public static async Task RegularAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("予報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                var datastore2 = new JmaDatastore2(AppIni.ProjectId);
                DateTime? update;
                if (AppIni.IsOutputToPostgreSQL2)
                {
                    update = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaRegularFeeds2")?.Update.ToUniversalTime();
                }
                else if (AppIni.IsOutputToDatastore2)
                {
                    update = await datastore2.GetUpdateAsync("JmaXmlInfo", "JmaRegularFeeds2");
                }
                else
                    return;

                var dt = Utils.GetForecastTime("vpfw50").AddMinutes(-30);
                if (update == null || update < dt)
                    update = dt;

                var list = await datastore2.GetJmaFeed("regular", (DateTime)update);
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


               
                if (AppIni.IsOutputToPostgreSQL2)
                {
                    /*
                    await PostgreUpsertData(vpfg50List, forecastContext, "jma_vpfg50", "json_vpfg50", JsonVpfg50);
                    await PostgreUpsertData(vpfw50List, forecastContext, "jma_vpfw50", "json_vpfw50", JsonVpfw50);
                    await PostgreUpsertData(vpfd50List, forecastContext, "jma_vpfd50", "json_vpfd50", JsonCondition);
                    await PostgreUpsertData(vpcw50List, forecastContext, "jma_vpcw50", "json_vpcw50", JsonCondition);
                    await PostgreUpsertData(vpzw50List, forecastContext, "jma_vpzw50", "json_vpzw50", JsonCondition);
                    PostgreSetUpdate(forecastContext, lastUpdate);
                    */
                }
               

                if (AppIni.IsOutputToDatastore2)
                {

                    //電文を受け取れなかった場合、PubSubHubbubは再送をしてくれる。
                    //その場合受けとる順番はランダムになるため、新しい電文を古い電文で置き換えないかチェック
                    List<(int id, long update)?>[] updateList = new List<(int id, long update)?>[Enum.GetNames(typeof(JmaForecastTask)).Length];
                    foreach (JmaForecastTask task in Enum.GetValues(typeof(JmaForecastTask)))
                    {
                        if(feedList.Any(x => x.Task == task.ToString()))
                        {
                            updateList[(int)task] = await datastore2.GetJmaUpdateAsync(task.ToString());
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
                    await datastore2.SetUpdateAsync("JmaXmlInfo", "JmaRegularFeeds2", lastUpdate);
                }

                await Utils.WriteLog("予報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        static async Task PostgreUpsertData(List<JmaFeedData> forecastList, ForecastContext forecastContext, string xmlTable, string jsonTable, Func<string, int, string> func)
        {
            if (!forecastList.Any())
                return;

            string sql1 = $"INSERT INTO {xmlTable}(id, forecast, update) VALUES(@id, @forecast, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";
            string sql2 = $"INSERT INTO {jsonTable}(id, forecast, update) VALUES(@id, @forecast, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            foreach (var f in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(f.Link);

                NpgsqlParameter id = new NpgsqlParameter("id", f.Id);
                NpgsqlParameter update = new NpgsqlParameter("update", f.UpdateTime);
                NpgsqlParameter forecast = new NpgsqlParameter("forecast", xml);

                int num = forecastContext.Database.ExecuteSqlCommand(sql1, id, forecast, update);

                string json = func(xml, f.Id);
                forecast = new NpgsqlParameter("forecast", json);
                forecastContext.Database.ExecuteSqlCommand(sql2, id, forecast, update);
            }
        }

        static void PostgreSetUpdate(ForecastContext forecastContext, DateTime lastUpdate)
        {
            NpgsqlParameter id = new NpgsqlParameter("id", "JmaRegularFeeds");
            NpgsqlParameter update = new NpgsqlParameter("update", lastUpdate);

            string sql = $"INSERT INTO jma_xml_info(id, update) VALUES(@id, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET update = EXCLUDED.update;";
            forecastContext.Database.ExecuteSqlCommand(sql, id, update);
        }

        //天気予報等の処理-Datastoreを使用
        static async Task UpsertData(List<JmaFeedData2> forecastList)
        {
            var datastore1 = new JmaDatastore2(AppIni.ProjectId);
            var datastore2 = new JmaDatastore2(AppIni.ProjectId);
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
