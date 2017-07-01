using Google.Cloud.Datastore.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmaXmlClient.Models
{
    public class JmaXmlExtra
    {
        public const string KindWarningXml = "JmaWarningXml";
        public static async Task ExtraAsync()
        {
            await Utils.WriteLog("注意報開始");
            var vpww53List = new List<JmaXmlData>(); 
            var vpww54List = new List<JmaXmlData>(); 

            try
            {
                var datastore1 = new Datastore("JmaXmlInfo");
                DateTime? update = await datastore1.GetUpdateAsync("JmaExtraFeeds");

                if (update == null || update < DateTime.UtcNow.AddHours(-24))
                    update = DateTime.UtcNow.AddHours(-24);

                var datastore = new Datastore("JmaXmlExtra");
                var list = await datastore.GetJmaFeed("JmaXmlExtra", (DateTime)update);
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
                            case "vpww53": //気象特別警報・警報・注意報
                                Utils.AddFeed(vpww53List, feed);
                                break;
                            case "vpww54": //気象警報・注意報（Ｈ２７）
                                Utils.AddFeed(vpww54List, feed);
                                break;
                        }
                    }
                }

                await UpsertData(vpww53List, "JmaVpww53");
                await UpsertData(vpww54List, "JmaVpww54");

                await datastore1.SetUpdateAsync("JmaExtraFeeds", lastUpdate);

                await Utils.WriteLog("注意報終了");
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("エラー発生: " + e1.Message);
            }
        }


        private static async Task UpsertData(List<JmaXmlData> forecastList, string kindXml)
        {
            if (!forecastList.Any())
                return;

            Datastore datastore = new Datastore(kindXml);
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