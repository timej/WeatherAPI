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
    public class JmaPgRegularTask
    {
        public static async Task RegularAsync(ForecastContext forecastContext)
        {
            await Utils.WriteLog("予報開始");
            var feedList = new List<JmaFeedData2>();
            try
            {
                DateTime? update = forecastContext.JmaXmlInfo.FirstOrDefault(x => x.Id == "JmaRegularFeeds2")?.Update.ToUniversalTime();

                var dt = Utils.GetForecastTime("vpfw50").AddMinutes(-30);
                if (update == null || update < dt)
                    update = dt;

                var list = forecastContext.JmaXmlRegular.Where(x => x.Created > update).OrderByDescending(x => x.Created);
                if (!list.Any())
                    return;
                DateTime lastUpdate = list.First().Created;

                foreach (var xmlRegular in list)
                {
                    string s = xmlRegular.Feeds;
                    var feeds = JsonConvert.DeserializeObject<List<JmaXmlFeed>>(s);
                    foreach (var feed in feeds)
                    {
                        Utils.AddFeed(feedList, feed);
                    }
                }

               
                await PostgreUpsertData(feedList, forecastContext);
                    
                PostgreSetUpdate(forecastContext, lastUpdate);
                await Utils.WriteLog("予報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }

        static async Task PostgreUpsertData(List<JmaFeedData2> feedList, ForecastContext forecastContext)
        {
            if (!feedList.Any())
                return;


            string sql1 = $"INSERT INTO jma_xml(task, id, forecast, update) VALUES(@task, @id, @forecast, @update) " +
                $"ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";
            string sql2 = $"INSERT INTO jma_json(task, id, forecast, update) VALUES(@task, @id, @forecast, @update) " +
                $"ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            foreach (var f in feedList)
            {
                DateTime? updateTime = forecastContext.JmaXml.FirstOrDefault(x => x.Task == f.Task && x.Id == f.Id)?.Update;
                if (updateTime == null || (DateTime)updateTime < f.UpdateTime)
                {
                    string xml = await JmaHttpClient.GetJmaXml(f.Link);

                    NpgsqlParameter task = new NpgsqlParameter("task", f.Task);
                    NpgsqlParameter id = new NpgsqlParameter("id", f.Id);
                    NpgsqlParameter update = new NpgsqlParameter("update", f.UpdateTime);
                    NpgsqlParameter forecast = new NpgsqlParameter("forecast", xml);

                    int num = forecastContext.Database.ExecuteSqlCommand(sql1, task, id, forecast, update);

                    string json = f.Task == "vpfd50" ? JsonVpfd50(xml, f.Id) 
                        : f.Task == "vpfw50" ? JsonVpfw50(xml, f.Id) : JsonCondition(xml, f.Id);

                    forecast = new NpgsqlParameter("forecast", json);
                    forecastContext.Database.ExecuteSqlCommand(sql2, task, id, forecast, update);
                }
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
            NpgsqlParameter id = new NpgsqlParameter("id", "JmaRegularFeeds2");
            NpgsqlParameter update = new NpgsqlParameter("update", lastUpdate);

            string sql = $"INSERT INTO jma_xml_info(id, update) VALUES(@id, @update) " +
                $"ON CONFLICT(id) DO UPDATE SET update = EXCLUDED.update;";
            forecastContext.Database.ExecuteSqlCommand(sql, id, update);
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
