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
    public class JmaXmlExtraTask
    {
        public static async Task ExtraAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("注意報開始");
            var vpww53List = new List<JmaFeedData>(); 
            var vpww54List = new List<JmaFeedData>(); 

            try
            {
                var datastore1 = new JmaDatastore(AppIni.ProjectId, "JmaXmlInfo");
                DateTime? updateUtc;

                if (AppIni.IsOutputToPostgreSQL)
                {
                    updateUtc = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaExtraFeeds")?.Update.ToUniversalTime();
                }
                else if (AppIni.IsOutputToDatastore)
                {
                    updateUtc = (await datastore1.GetUpdateAsync("JmaExtraFeeds"))?.ToUniversalTime();
                }
                else
                    return;

                if (updateUtc == null || updateUtc < DateTime.UtcNow.AddDays(-1))
                    updateUtc = DateTime.UtcNow.AddDays(-1);

                var datastore = new JmaDatastore(AppIni.ProjectId, "JmaXmlExtra");
                var list = await datastore.GetJmaFeed("JmaXmlExtra", (DateTime)updateUtc);
                if (!list.Any())
                    return;
                DateTime lastUpdateUtc = list.First().Properties["created"].TimestampValue.ToDateTime().ToUniversalTime();

                foreach (var xmlRegular in list)
                {
                    string json = xmlRegular.Properties["feeds"].StringValue;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(json);
                    foreach (var feed in feeds)
                    {
                        switch (feed.Task)
                        {
                            case "vpww53": //気象特別警報・警報・注意報
                                Utils.AddFeed(vpww53List, feed);
                                break;
                            case "vpww54": //気象警報・注意報（Ｈ２７）
                                Utils.AddFeed(vpww54List, feed);
                                break;
                        }
                    }
                }

                if (AppIni.IsOutputToPostgreSQL)
                {
                    await PostgreUpsertData(vpww53List, forecastContext, "jma_vpww53");
                    await PostgreUpsertData(vpww54List, forecastContext, "jma_vpww54");
                    PostgreSetUpdate(forecastContext, lastUpdateUtc);
                }

                    if (AppIni.IsOutputToDatastore)
                {
                    await UpsertData(vpww53List, "JmaVpww53");
                    await UpsertData(vpww54List, "JmaVpww54");
                    await datastore1.SetUpdateAsync("JmaExtraFeeds", lastUpdateUtc);
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

        static void PostgreSetUpdate(ForecastContext forecastContext, DateTime lastUpdate)
        {
            NpgsqlParameter id = new NpgsqlParameter("id", "JmaExtraFeeds");
            NpgsqlParameter update = new NpgsqlParameter("update", lastUpdate);

            string sql = $"INSERT INTO jma_xml_info(id, update) VALUES(@id, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET update = EXCLUDED.update;";
            forecastContext.Database.ExecuteSqlCommand(sql, id, update);
        }

        private static async Task UpsertData(List<JmaFeedData> forecastList, string kindXml)
        {
            if (!forecastList.Any())
                return;

            var datastore = new JmaDatastore(AppIni.ProjectId, kindXml);
            var entityList = new List<Entity>();

            foreach (var forecast in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(forecast.Link);
                entityList.Add(datastore.SetEntity(forecast.Id, xml, forecast.UpdateTime.ToUniversalTime()));
            }

            await datastore.UpsertForecastAsync(entityList);
        }
    }
}