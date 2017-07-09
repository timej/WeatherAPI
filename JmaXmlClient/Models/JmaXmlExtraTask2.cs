using Google.Cloud.Datastore.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JmaXml.Common.Data;
using JmaXml.Common;

namespace JmaXmlClient.Models
{
    public class JmaXmlExtraTask2
    {
        enum JmaWarningTask { vpww53, vpww54 };
        public static async Task ExtraAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("注意報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                var datastore2 = new JmaDatastore2(AppIni.ProjectId);
                DateTime? updateUtc;
                if (AppIni.IsOutputToPostgreSQL2)
                {
                    updateUtc = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaExtraFeeds2")?.Update.ToUniversalTime();
                }
                else if (AppIni.IsOutputToDatastore2)
                {
                    updateUtc = (await datastore2.GetUpdateAsync("JmaXmlInfo", "JmaExtraFeeds2"))?.ToUniversalTime();
                }
                else
                    return;

                var dt = DateTime.UtcNow.AddDays(-1);
                if (updateUtc == null || updateUtc < dt)
                    updateUtc = dt;

                var list = await datastore2.GetJmaFeed("extra", (DateTime)updateUtc);
                if (!list.Any())
                    return;


                DateTime lastUpdateUtc = list.First().Properties["created"].TimestampValue.ToDateTime().ToUniversalTime();

                foreach (var xmlRegular in list)
                {
                    string json = xmlRegular.Properties["feeds"].StringValue;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(json);
                    foreach (var feed in feeds)
                    {
                        Utils.AddFeed(feedList, feed);
                    }
                }

                if (AppIni.IsOutputToPostgreSQL2)
                {
                    /*
                    await PostgreUpsertData(vpww53List, forecastContext, "jma_vpww53");
                    await PostgreUpsertData(vpww54List, forecastContext, "jma_vpww54");
                    */
                }

                if (AppIni.IsOutputToDatastore2)
                {

                    //電文を受け取れなかった場合、PubSubHubbubは再送をしてくれる。
                    //その場合受けとる順番はランダムになるため、新しい電文を古い電文で置き換えないかチェック
                    List<(int id, long update)?>[] updateList = new List<(int id, long update)?>[Enum.GetNames(typeof(JmaWarningTask)).Length];
                    foreach (JmaWarningTask task in Enum.GetValues(typeof(JmaWarningTask)))
                    {
                        if (feedList.Any(x => x.Task == task.ToString()))
                        {
                            updateList[(int)task] = await datastore2.GetJmaUpdateAsync(task.ToString());
                        }
                    }

                    var feedList1 = new List<JmaFeedData2>();
                    foreach (var feed in feedList)
                    {
                        var previous = updateList[(int)Enum.Parse(typeof(JmaWarningTask), feed.Task)].FirstOrDefault(x => x?.id == feed.Id);
                        long l = Utils.UnixTime(feed.UpdateTime);
                        if (previous == null || previous?.update < l)
                            feedList1.Add(feed);
                    }


                    await UpsertData(feedList1);
            
                    await datastore2.SetUpdateAsync("JmaXmlInfo", "JmaExtraFeeds2", lastUpdateUtc);
                }

                await Utils.WriteLog("注意報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        private static async Task PostgreUpsertData(List<JmaFeedData> forecastList, ForecastContext forecastContext, string xmlTable)
        {
            if (!forecastList.Any())
                return;

            if (!forecastList.Any())
                return;

            string sql1 = $"INSERT INTO {xmlTable}(id, forecast, update) VALUES(@id, @forecast, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            foreach (var f in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(f.Link);

                NpgsqlParameter id = new NpgsqlParameter("id", f.Id);
                NpgsqlParameter update = new NpgsqlParameter("update", f.UpdateTime);
                NpgsqlParameter forecast = new NpgsqlParameter("forecast", xml);

                int num = forecastContext.Database.ExecuteSqlCommand(sql1, id, forecast, update);
            }
        }

        private static async Task UpsertData(List<JmaFeedData2> forecastList)
        {
     
            var datastore = new JmaDatastore2(AppIni.ProjectId);
            var entityList = new List<Entity>();

            foreach (var forecast in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(forecast.Link);
                entityList.Add(datastore.SetEntity("JmaXml", forecast.Task, forecast.Id, xml, forecast.UpdateTime.ToUniversalTime()));
            }

            await datastore.UpsertForecastAsync(entityList);
        }
    }
}