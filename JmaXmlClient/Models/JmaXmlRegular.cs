using Google.Cloud.Datastore.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JmaXmlClient.Models;
using JmaXmlClient.Data;
using System.Data.SqlClient;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace JmaXmlClient.Models
{
    public class JmaXmlRegular
    {
        public static async Task RegularAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("予報開始");
            var vpfg50List = new List<JmaXmlData>();
            var vpfd50List = new List<JmaXmlData>();
            var vpfw50List = new List<JmaXmlData>();
            var vpcw50List = new List<JmaXmlData>();
            var vpzw50List = new List<JmaXmlData>();
            try
            {
                Datastore datastore = new Datastore("JmaXmlInfo");
                DateTime? update;
                if (AppIni.IsOutputToPostgreSQL)
                {
                    update = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaRegularFeeds")?.Update.ToUniversalTime();
                }
                else if (AppIni.IsOutputToDatastore)
                {
                    update = await datastore.GetUpdateAsync("JmaRegularFeeds");
                }
                else
                    return;

                if (update == null || update < Utils.GetForecastTime("vpfw50").AddMinutes(-30))
                    update = Utils.GetForecastTime("vpfw50").AddMinutes(-30);

                var list = await datastore.GetJmaFeed("JmaXmlRegular", (DateTime)update);
                if (!list.Any())
                    return;
                DateTime lastUpdate = list.First().Properties["created"].TimestampValue.ToDateTime();

                foreach (var xmlRegular in list)
                {
                    string json = xmlRegular.Properties["feeds"].StringValue;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(json);
                    foreach (var feed in feeds)
                    {
                        switch (feed.Task)
                        {
                            case "vpfg50": //府県天気予報
                                Utils.AddFeed(vpfg50List, feed);
                                break;
                            case "vpfd50": //府県天気概況
                                Utils.AddFeed(vpfd50List, feed);
                                break;
                            case "vpfw50": //府県週間天気予報
                                Utils.AddFeed(vpfw50List, feed);
                                break;
                            case "vpcw50": //地方週間天気予報
                                Utils.AddFeed(vpcw50List, feed);
                                break;
                            case "vpzw50": //全般週間天気予報
                                Utils.AddFeed(vpzw50List, feed);
                                break;
                        }
                    }
                }
                if (AppIni.IsOutputToPostgreSQL)
                {
                    await PostgreUpsertData(vpfg50List, forecastContext, "jma_vpfg50", "json_vpfg50", JsonVpfg50);
                    await PostgreUpsertData(vpfw50List, forecastContext, "jma_vpfw50", "json_vpfw50", JsonVpfw50);
                    await PostgreUpsertData(vpfd50List, forecastContext, "jma_vpfd50", "json_vpfd50", JsonCondition);
                    await PostgreUpsertData(vpcw50List, forecastContext, "jma_vpcw50", "json_vpcw50", JsonCondition);
                    await PostgreUpsertData(vpzw50List, forecastContext, "jma_vpzw50", "json_vpzw50", JsonCondition);
                    PostgreSetUpdate(forecastContext, lastUpdate);
                }

                if (AppIni.IsOutputToDatastore)
                {
                    //府県天気予報
                    await UpsertData(vpfg50List, "JmaVpfg50", "JsonVpfg50", JsonVpfg50);
                    //府県週間天気予報
                    await UpsertData(vpfw50List, "JmaVpfw50", "JsonVpfw50", JsonVpfw50);
                    //府県天気概況
                    await UpsertData(vpfd50List, "JmaVpfd50", "JsonVpfd50", JsonCondition);
                    await UpsertData(vpcw50List, "JmaVpcw50", "JsonVpcw50", JsonCondition);
                    await UpsertData(vpzw50List, "JmaVpzw50", "JsonVpzw50", JsonCondition);

                    await datastore.SetUpdateAsync("JmaRegularFeeds", lastUpdate);
                }

                await Utils.WriteLog("予報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        static async Task PostgreUpsertData(List<JmaXmlData> forecastList, ForecastContext forecastContext, string xmlTable, string jsonTable, Func<string, int, string> func)
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

        //府県天気予報の処理
        static async Task UpsertData(List<JmaXmlData> forecastList, string kindXml, string kindJson, Func<string, int, string> func)
        {
            if (!forecastList.Any())
                return;

            Datastore datastore1 = new Datastore(kindXml);
            Datastore datastore2 = new Datastore(kindJson);
            var entityList1 = new List<Entity>();
            var entityList2 = new List<Entity>();

            foreach (var forecast in forecastList)
            {
                string xml = await JmaHttpClient.GetJmaXml(forecast.Link);
                entityList1.Add(datastore1.SetEntity(forecast.Id, xml, forecast.UpdateTime.ToUniversalTime()));
                string json = func(xml, forecast.Id);
                entityList2.Add(datastore2.SetEntity(forecast.Id, json, forecast.UpdateTime.ToUniversalTime()));
            }

            await datastore1.UpsertForecastAsync(entityList1);
            await datastore2.UpsertForecastAsync(entityList2);
        }


        static string JsonVpfg50(string xml, int id)
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
