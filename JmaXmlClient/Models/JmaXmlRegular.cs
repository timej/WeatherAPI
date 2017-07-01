using Google.Cloud.Datastore.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmaXmlClient.Models
{
    public class JmaXmlRegular
    {
        public static async Task RegularAsync()
        {
            await Utils.WriteLog("予報開始");
            var vpfg50List = new List<JmaXmlData>();
            var vpfd50List = new List<JmaXmlData>();
            var vpfw50List = new List<JmaXmlData>();
            var vpcw50List = new List<JmaXmlData>();
            var vpzw50List = new List<JmaXmlData>();
            try
            {
                var datastore = new Datastore("JmaXmlInfo");
                DateTime? update = await datastore.GetUpdateAsync("JmaRegularFeeds");
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

                await UpsertData(vpfg50List, "JmaVpfg50", "JsonVpfg50", JsonVpfg50);
                await UpsertData(vpfd50List, "JmaVpfd50", "JsonVpfd50", JsonVpfw50);
                await UpsertData(vpfw50List, "JmaVpfw50", "JsonVpfw50", JsonCondition);
                await UpsertData(vpcw50List, "JmaVpcw50", "JsonVpcw50", JsonCondition);
                await UpsertData(vpzw50List, "JmaVpzw50", "JsonVpzw50", JsonCondition);

                await datastore.SetUpdateAsync("JmaRegularFeeds", lastUpdate);

                await Utils.WriteLog("予報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
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
