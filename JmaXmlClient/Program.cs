using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JmaXmlClient.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace JmaXmlClient
{
    class Program
    {
        static Program()
        {
            ApplicationEnvironment env = PlatformServices.Default.Application;

            string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "linux";
            var builder = new ConfigurationBuilder();
            builder
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{os}.json", optional: true);

            var configuration = builder.Build();

            AppIni.Init(env.ApplicationBasePath, configuration);
        }
        static void Main(string[] args)
        {
            //Windows のコマンドプロンプトが既定ではSift_JISコードのための対応
#if DEBUG
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            //天気予報等
            if(args.Contains("-r"))
                JmaXmlRegular.RegularAsync().GetAwaiter().GetResult();
            //警報・注意報等
            else if(args.Contains("-e"))
                JmaXmlExtra.ExtraAsync().GetAwaiter().GetResult();
            //XMLデータから天気予報のJsonデータの一括作成
            else if (args.Contains("-a"))
                MakeJsonData().GetAwaiter().GetResult();
            //XMLデータからWeeklyJsonデータの一括作成
            else if (args.Contains("-w"))
                MakeWeeklyJsonData().GetAwaiter().GetResult();
            //XMLデータから天気概況のJsopnデータの一括作成
            else if (args.Contains("-g"))
                WeatherConditionJsonData().GetAwaiter().GetResult();
            //古いデータの削除
            else if (args.Contains("-d"))
                DeleteData().GetAwaiter().GetResult();
            else
                MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
#if !DEBUG
            await Utils.WriteLog("開始");
#endif
            int office = 360;
            var datastore = new Datastore("JmaVpfg50");
            string xml = await datastore.GetJmaXmlAsync(office);
            var forecast = new JmaForecast(xml, office);
            
#if !DEBUG
            await Utils.WriteLog("終了");
#endif
        }

        static async Task DeleteData()
        {
            var datastore = new Datastore("JmaXmlExtra");
            await datastore.DeleteFeedsAsync("JmaXmlExtra");
            var datastore2 = new Datastore("JmaXmlRegular");
            await datastore2.DeleteFeedsAsync("JmaXmlRegular");
        }

        //天気予報XMLデータから一括して天気予報Jsonデータを作成
        static async Task MakeJsonData()
        {
            await Utils.WriteLog("天気予報Jsonデータ一括作成開始");
            //入力側
            var datastore = new Datastore("JmaVpfg50");
            //保存側
            Datastore datasore2 = new Datastore("JsonVpfg50");
            var entityList = new List<Google.Cloud.Datastore.V1.Entity>();

            var results = await datastore.GetAllJmaXmlAsync("JmaVpfg50");
            foreach (var entity in results.Entities)
            {
                string xml = entity.Properties["forecast"].StringValue;
                var dt = entity.Properties["update"].TimestampValue;
                int id = (int)entity.Key.Path[0].Id;
                JmaForecast jmaForecast = new JmaForecast(xml, id);
                entityList.Add(datasore2.SetEntity(id, JsonConvert.SerializeObject(jmaForecast), dt.ToDateTime()));
            }
            await datasore2.UpsertForecastAsync(entityList);
            await Utils.WriteLog("天気予報Jsonデータ一括作成終了");
        }

        //週間天気予報XMLデータから一括して週間天気予報Jsonデータを作成
        static async Task MakeWeeklyJsonData()
        {
            await Utils.WriteLog("週間予報Jsonデータ一括作成開始");
            //入力側
            var datastore = new Datastore("JmaVpfw50");
            var results = await datastore.GetAllJmaXmlAsync("JmaVpfw50");
            //保存側
            Datastore datasore2 = new Datastore("JsonVpfw50");
            var entityList = new List<Google.Cloud.Datastore.V1.Entity>();
            foreach (var entity in results.Entities)
            {
                string xml = entity.Properties["forecast"].StringValue;
                var dt = entity.Properties["update"].TimestampValue;
                int id = (int)entity.Key.Path[0].Id;
                Weekly weekly = new Weekly(xml, id);
                entityList.Add(datasore2.SetEntity(id, JsonConvert.SerializeObject(weekly), dt.ToDateTime()));
            }
            await datasore2.UpsertForecastAsync(entityList);
            await Utils.WriteLog("週間予報Jsonデータ一括作成終了");
        }

        //府県天気概況XMLデータから一括して府県天気概況Jsonデータを作成
        static async Task WeatherConditionJsonData()
        {
            await Utils.WriteLog("天気概況Jsonデータ一括作成開始");

            await ConditionXmlToJson("JmaVpfd50", "JsonVpfd50");
            await ConditionXmlToJson("JmaVpcw50", "JsonVpcw50");
            await ConditionXmlToJson("JmaVpzw50", "JsonVpzw50");

            await Utils.WriteLog("天気概況Jsonデータ一括作成終了");
        }

        static async Task ConditionXmlToJson(string kindXml, string kindJson)
        {
            //入力側
            var datastore1 = new Datastore(kindXml);
            //保存側
            Datastore datastore2 = new Datastore(kindJson);
            var entityList = new List<Google.Cloud.Datastore.V1.Entity>();

            var results = await datastore1.GetAllJmaXmlAsync(kindXml);

            foreach (var entity in results.Entities)
            {
                string xml = entity.Properties["forecast"].StringValue;
                var dt = entity.Properties["update"].TimestampValue;
                int id = (int)entity.Key.Path[0].Id;
                var conditions = new WeatherConditions(xml, id);
                entityList.Add(datastore2.SetEntity(id, JsonConvert.SerializeObject(conditions), dt.ToDateTime()));
            }
            await datastore2.UpsertForecastAsync(entityList);
        }
    }
}