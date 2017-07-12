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
    public class JmaDsExtraTask
    {
        public static async Task ExtraAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("注意報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                DateTime? updateUtc= (await datastore.GetUpdateAsync("JmaXmlInfo", "JmaExtraFeeds2" ))?.ToUniversalTime();

                var dt = DateTime.UtcNow.AddDays(-1);
                if (updateUtc == null || updateUtc < dt)
                    updateUtc = dt;

                var list = await datastore.GetJmaFeed("extra", (DateTime)updateUtc);
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

                //電文を受け取れなかった場合、PubSubHubbubは再送をしてくれる。
                //その場合受けとる順番はランダムになるため、新しい電文を古い電文で置き換えないかチェック
                List<(int id, long update)?>[] updateList = new List<(int id, long update)?>[Enum.GetNames(typeof(JmaWarningTask)).Length];
                foreach (JmaWarningTask task in Enum.GetValues(typeof(JmaWarningTask)))
                {
                    if (feedList.Any(x => x.Task == task.ToString()))
                    {
                        updateList[(int)task] = await datastore.GetJmaUpdateAsync(task.ToString());
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
            
                await datastore.SetUpdateAsync("JmaXmlInfo", "JmaExtraFeeds2", lastUpdateUtc);                

                await Utils.WriteLog("注意報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        private static async Task UpsertData(List<JmaFeedData2> forecastList)
        {
     
            var datastore = new JmaDatastore(AppIni.ProjectId);
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