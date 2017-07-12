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
    public class JmaPgExtraTask
    {
        public static async Task ExtraAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("注意報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                DateTime? updateUtc = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaExtraFeeds2")?.Update.ToUniversalTime();

                var dt = DateTime.UtcNow.AddDays(-1);
                if (updateUtc == null || updateUtc < dt)
                    updateUtc = dt;

                var list = forecastContext.JmaXmlExtra.Where(x => x.Created > updateUtc).OrderByDescending(x => x.Created);
                if (!list.Any())
                    return;


                DateTime lastUpdateUtc = list.First().Created.ToUniversalTime();

                foreach (var xmlRegular in list)
                {
                    string json = xmlRegular.Feeds;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(json);
                    foreach (var feed in feeds)
                    {
                        Utils.AddFeed(feedList, feed);
                    }
                }

                    
                await PostgreUpsertData(feedList, forecastContext);
                PostgreSetUpdate(forecastContext, lastUpdateUtc);

                await Utils.WriteLog("注意報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        private static async Task PostgreUpsertData(List<JmaFeedData2> forecastList, ForecastContext forecastContext)
        {
            if (!forecastList.Any())
                return;

            string sql1 = $"INSERT INTO jma_xml(task, id, forecast, update) VALUES(@task, @id, @forecast, @update) " +
                $"ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            foreach (var f in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(f.Link);

                NpgsqlParameter task = new NpgsqlParameter("task", f.Task);
                NpgsqlParameter id = new NpgsqlParameter("id", f.Id);
                NpgsqlParameter update = new NpgsqlParameter("update", f.UpdateTime);
                NpgsqlParameter forecast = new NpgsqlParameter("forecast", xml);

                int num = forecastContext.Database.ExecuteSqlCommand(sql1, task, id, forecast, update);
            }
        }

        static void PostgreSetUpdate(ForecastContext forecastContext, DateTime lastUpdate)
        {
            NpgsqlParameter id = new NpgsqlParameter("id", "JmaExtraFeeds2");
            NpgsqlParameter update = new NpgsqlParameter("update", lastUpdate);

            string sql = $"INSERT INTO jma_xml_info(id, update) VALUES(@id, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET update = EXCLUDED.update;";
            forecastContext.Database.ExecuteSqlCommand(sql, id, update);
        }
    }
}