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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using JmaXml.Common.Data;
using Npgsql;
using JmaXml.Common;

namespace JmaXmlClient
{
    class Program
    {
        static IConfigurationRoot Configuration;
        static readonly ApplicationEnvironment _env;
        static ForecastContext _forecastContext;

        static Program()
        {
            _env = PlatformServices.Default.Application;

            string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "linux";
            var builder = new ConfigurationBuilder();
            builder
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{os}.json", optional: true);

            Configuration = builder.Build();

        }

        public static void ConfigureServices(IServiceCollection services)
        {
            if (bool.Parse(Configuration["Database:PostgreSQL"]))
            {
                services.AddDbContext<ForecastContext>(options => options.UseNpgsql(Configuration.GetConnectionString("ForecastConnection")));
            }
        }

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            if (bool.Parse(Configuration["Database:PostgreSQL"]))
            {
                _forecastContext = serviceProvider.GetService<ForecastContext>();
                //データベースの自動作成
                //_forecastContext.Database.Migrate();
            }


            AppIni.Init(_env.ApplicationBasePath, Configuration);

            //Windows のコマンドプロンプトが既定ではSift_JISコードのための対応
#if DEBUG
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            //天気予報等
            if (args.Contains("-r"))
            {
                if (AppIni.IsUseDatastore)
                    JmaDsRegularTask.RegularAsync().GetAwaiter().GetResult();
                if (AppIni.IsUsePostgreSQL)
                    JmaPgRegularTask.RegularAsync(_forecastContext).GetAwaiter().GetResult();
            }
            //警報・注意報等
            else if (args.Contains("-e"))
            {
                if (AppIni.IsUseDatastore)
                    JmaDsExtraTask.ExtraAsync().GetAwaiter().GetResult();
                if (AppIni.IsUsePostgreSQL)
                    JmaPgExtraTask.ExtraAsync(_forecastContext).GetAwaiter().GetResult();
            }
            //気象庁防災情報XML電文のPULL型を利用する場合
            else if (args.Contains("-p"))
            {
                if (AppIni.IsUseDatastore)
                    JmaPull.PullAsync().GetAwaiter().GetResult();
            }
            else if (args.Contains("-q"))
            {
                if (AppIni.IsUseDatastore)
                    JmaPull.PullLAsync().GetAwaiter().GetResult();
            }
            //XMLデータから天気予報のJsonデータの一括作成
            else if (args.Contains("-a"))
            {
                MakeJsonData().GetAwaiter().GetResult();
            }
            //XMLデータからWeeklyJsonデータの一括作成
            else if (args.Contains("-w"))
            {
                MakeWeeklyJsonData().GetAwaiter().GetResult();
            }
            //XMLデータから天気概況のJsopnデータの一括作成
            else if (args.Contains("-g"))
            {
                WeatherConditionJsonData().GetAwaiter().GetResult();
            }
            //サマリーの作成
            else if (args.Contains("-s"))
            {
                SetSummary().GetAwaiter().GetResult();
            }
            //サマリーの保存
            else if (args.Contains("-t"))
            {
                SetSummarySave().GetAwaiter().GetResult();
            }
            //サマリーのチェック
            else if (args.Contains("-k"))
            {
                SummaryCheck().GetAwaiter().GetResult();
            }
            //週間予報サマリーの作成
            else if (args.Contains("-m"))
            {
                SetWeeklySummary().GetAwaiter().GetResult();
            }
            //週間予報サマリーの保存
            else if (args.Contains("-n"))
            {
                SetWeeklySummarySave().GetAwaiter().GetResult();
            }
            //週間予報サマリーのチェック
            else if (args.Contains("-o"))
            {
                WeeklySummaryCheck().GetAwaiter().GetResult();
            }
            //古いデータの削除
            else if (args.Contains("-d"))
            {
                DeleteData().GetAwaiter().GetResult();
            }
            else
                MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
#if !DEBUG
            await Utils.WriteLog("開始");
#endif

            int office = 360;
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string xml = await datastore.GetJmaXmlAsync("vpfd50", office);
            var forecast = new JmaForecast(xml, office);

#if !DEBUG
            await Utils.WriteLog("終了");
#endif
        }

        static async Task DeleteData()
        {
            int hour = 24; //24時間より前の更新情報を削除
            if (AppIni.IsUsePostgreSQL)
            {
                string sql = $"DELETE FROM public.jma_xml_extra WHERE created < '{DateTime.UtcNow.AddHours(-hour).ToString("yyyy-MM-ddTHH:mm:ssZ")}'; ";
                _forecastContext.Database.ExecuteSqlCommand(sql);

                sql = $"DELETE FROM public.jma_xml_regular WHERE created < '{DateTime.UtcNow.AddHours(-hour).ToString("yyyy-MM-ddTHH:mm:ssZ")}'; ";
                _forecastContext.Database.ExecuteSqlCommand(sql);
            }

            if (AppIni.IsUseDatastore)
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                await datastore.DeleteFeedsAsync(-hour);
            }
        }

        //天気予報XMLデータから一括して天気予報Jsonデータを作成
        static async Task MakeJsonData()
        {
            await Utils.WriteLog("天気予報Jsonデータ一括作成開始");

            if(AppIni.IsUsePostgreSQL)
            {
                string sql = "INSERT INTO jma_json (task, id, forecast, update) VALUES('vpfd50', @id, @forecast, @update) " +
                    "ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

                var options = new DbContextOptionsBuilder<ForecastContext>()
                    .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));
               
                using (var context2 = new ForecastContext(options.Options))
                {

                    foreach (var data in _forecastContext.JmaXml.Where(x => x.Task == "vpfd50"))
                    {
                        JmaForecast jmaForecast = new JmaForecast(data.Forecast, data.Id);
                        string json = JsonConvert.SerializeObject(jmaForecast);

                        NpgsqlParameter id = new NpgsqlParameter("id", data.Id);
                        NpgsqlParameter update = new NpgsqlParameter("update", data.Update);
                        NpgsqlParameter forecast = new NpgsqlParameter("forecast", json);

                        context2.Database.ExecuteSqlCommand(sql, id, forecast, update);
                    }
                }
            }

            if (AppIni.IsUseDatastore)
            {
                //入力側
                var datastore = new JmaDatastore(AppIni.ProjectId);
                var results = await datastore.GetAllJmaXmlAsync("JmaXml", "vpfd50");

                //保存側
                var datasore2 = new JmaDatastore(AppIni.ProjectId);
                var entityList = new List<Google.Cloud.Datastore.V1.Entity>();

                foreach (var entity in results.Entities)
                {
                    string xml = entity.Properties["forecast"].StringValue;
                    var dt = entity.Properties["update"].TimestampValue;
                    int id = (int)entity.Key.Path[1].Id;
                    JmaForecast jmaForecast = new JmaForecast(xml, id);
                    entityList.Add(datasore2.SetEntity("JmaJson", "vpfd50", id, JsonConvert.SerializeObject(jmaForecast), dt.ToDateTime()));
                }
                await datasore2.UpsertForecastAsync(entityList);
            }
            await Utils.WriteLog("天気予報Jsonデータ一括作成終了");
        }

        //週間天気予報XMLデータから一括して週間天気予報Jsonデータを作成
        static async Task MakeWeeklyJsonData()
        {
            await Utils.WriteLog("週間予報Jsonデータ一括作成開始");

            if (AppIni.IsUsePostgreSQL)
            {
                string sql = "INSERT INTO jma_json (task, id, forecast, update) VALUES('vpfw50', @id, @forecast, @update) " +
                    "ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

                var options = new DbContextOptionsBuilder<ForecastContext> ()
                    .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));

             
                using (var context2 = new ForecastContext(options.Options))
                {
                    foreach (var data in _forecastContext.JmaJson.Where(x => x.Task == "vpfw50"))
                    {
                        Weekly weekly = new Weekly(data.Forecast, data.Id);
                        string json = JsonConvert.SerializeObject(weekly);

                        NpgsqlParameter id = new NpgsqlParameter("id", data.Id);
                        NpgsqlParameter update = new NpgsqlParameter("update", data.Update);
                        NpgsqlParameter forecast = new NpgsqlParameter("forecast", json);

                        context2.Database.ExecuteSqlCommand(sql, id, forecast, update);
                    }
                }
            }

            if (AppIni.IsUseDatastore)
            {
                //入力側
                var datastore = new JmaDatastore(AppIni.ProjectId);
                var results = await datastore.GetAllJmaXmlAsync("JmaXml", "vpfw50");

                //保存側
                var datasore2 = new JmaDatastore(AppIni.ProjectId);
                var entityList = new List<Google.Cloud.Datastore.V1.Entity>();
                foreach (var entity in results.Entities)
                {
                    string xml = entity.Properties["forecast"].StringValue;
                    var dt = entity.Properties["update"].TimestampValue;
                    int id = (int)entity.Key.Path[1].Id;
                    Weekly weekly = new Weekly(xml, id);
                    entityList.Add(datasore2.SetEntity("JmaJson", "vpfw50", id, JsonConvert.SerializeObject(weekly), dt.ToDateTime()));
                }
                await datasore2.UpsertForecastAsync(entityList);
            }
            await Utils.WriteLog("週間予報Jsonデータ一括作成終了");
        }

        //府県天気概況XMLデータから一括して府県天気概況Jsonデータを作成
        static async Task WeatherConditionJsonData()
        {
            await Utils.WriteLog("天気概況Jsonデータ一括作成開始");

            if (AppIni.IsUsePostgreSQL)
            {
                PgConditionXmlToJson("vpfg50");
                PgConditionXmlToJson("vpcw50");
                PgConditionXmlToJson("vpzw50");
            }

            if (AppIni.IsUseDatastore)
            {
                await ConditionXmlToJson("vpfg50");
                await ConditionXmlToJson("vpcw50");
                await ConditionXmlToJson("vpzw50");
            }

            await Utils.WriteLog("天気概況Jsonデータ一括作成終了");
        }

        static void PgConditionXmlToJson(string jmaTask)
        {
            string sql = $"INSERT INTO jma_json (task, id, forecast, update) VALUES(@task, @id, @forecast, @update) " +
                "ON CONFLICT(task, id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            var options = new DbContextOptionsBuilder<ForecastContext>()
                .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));
          
           
            using (var context2 = new ForecastContext(options.Options))
            {

                foreach (var data in _forecastContext.JmaXml.Where(x => x.Task == jmaTask))
                {
                    WeatherConditions conditions = new WeatherConditions(data.Forecast, data.Id);
                    string json = JsonConvert.SerializeObject(conditions);

                    NpgsqlParameter task = new NpgsqlParameter("task", jmaTask);
                    NpgsqlParameter id = new NpgsqlParameter("id", data.Id);
                    NpgsqlParameter update = new NpgsqlParameter("update", data.Update);
                    NpgsqlParameter forecast = new NpgsqlParameter("forecast", json);

                    context2.Database.ExecuteSqlCommand(sql, task, id, forecast, update);
                }
            }
      
        }

        static async Task ConditionXmlToJson(string task)
        {
            //入力側
            var datastore1 = new JmaDatastore(AppIni.ProjectId);
            //保存側
            var datastore2 = new JmaDatastore(AppIni.ProjectId);
            var entityList = new List<Google.Cloud.Datastore.V1.Entity>();

            var results = await datastore1.GetAllJmaXmlAsync("JmaXml", task);

            foreach (var entity in results.Entities)
            {
                string xml = entity.Properties["forecast"].StringValue;
                var dt = entity.Properties["update"].TimestampValue;
                int id = (int)entity.Key.Path[1].Id;
                var conditions = new WeatherConditions(xml, id);
                entityList.Add(datastore2.SetEntity("JmaJson", task, id, JsonConvert.SerializeObject(conditions), dt.ToDateTime()));
            }
            await datastore2.UpsertForecastAsync(entityList);
        }

        static async Task SetSummary()
        {
            await Utils.WriteLog("天気予報サマリー作成開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            var dataList = await datastore.GetAllJmaXmlAsync("JmaJson", "vpfd50");
            var forcastList = new List<JmaForecast>();
            foreach(var data in dataList.Entities)
            {
                forcastList.Add(JsonConvert.DeserializeObject<JmaForecast>(data.Properties["forecast"].StringValue));
            }
            await JmaDsRegularTask.SetSummary(forcastList);
            await Utils.WriteLog("天気予報サマリー作成終了");
        }

        static async Task SetSummarySave()
        {
            await Utils.WriteLog("天気予報サマリー保存開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string json = await datastore.GetInfoDataAsync("forecastSummaries");
            await datastore.SetInfoDataAsnc("forecastSummaries" + GetForecastTime().ToString("yyyyMMddTHH") + "f", json, DateTime.UtcNow);
            await Utils.WriteLog("天気予報サマリー保存終了");
        }

        static async Task SummaryCheck()
        {
            await Utils.WriteLog("天気予報サマリーチェック開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string json = await datastore.GetInfoDataAsync("forecastSummaries");
            var jmaForecastSummary = JsonConvert.DeserializeObject<JmaForecastSummary>(json);
            if(jmaForecastSummary.TimeDefine[0].ToUniversalTime() != GetForecastTime())
            {
                json = await datastore.GetInfoDataAsync("forecastSummaries" + GetForecastTime().ToString("yyyyMMddTHH"));
                await datastore.SetInfoDataAsnc("forecastSummaries", json, DateTime.UtcNow);
                await Utils.WriteLog("天気予報サマリーの更新ができていませんでした。");
                return;
            }
            await Utils.WriteLog("天気予報サマリーチェック終了");
        }

        private static DateTime GetForecastTime()
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var h = now.Hour;
            if (now.Minute > 40)
                h++;
            //11時
            if (h < 2)
                return date.AddHours(-4);
            //17時
            else if (h < 8)
                return date.AddHours(2);
            //5時
            else if (h < 20)
                return date.AddHours(8);
            else
            {
                return date.AddHours(20);
            }
        }

        static async Task SetWeeklySummary()
        {
            await Utils.WriteLog("週間予報サマリー作成開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            var dataList = await datastore.GetAllJmaXmlAsync("JmaJson", "vpfw50");
            var forcastList = new List<Weekly>();
            foreach (var data in dataList.Entities)
            {
                forcastList.Add(JsonConvert.DeserializeObject<Weekly>(data.Properties["forecast"].StringValue));
            }
            await JmaDsRegularTask.SetWeeklySummary(forcastList);
            await Utils.WriteLog("週間予報サマリー作成終了");
        }

        //11時と17時の28分にRun
        static async Task SetWeeklySummarySave()
        {
            await Utils.WriteLog("週間予報サマリー保存開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string json = await datastore.GetInfoDataAsync("weeklySummaries");
            await datastore.SetInfoDataAsnc("weeklySummaries" + GetWeeklyForecastTime().ToString("yyyyMMddTHH") + "f", json, DateTime.UtcNow);
            await Utils.WriteLog("週間予報サマリー保存終了");
        }

        //11時と17時の58分にRun
        static async Task WeeklySummaryCheck()
        {
            await Utils.WriteLog("週間予報サマリーチェック開始");
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string json = await datastore.GetInfoDataAsync("weeklySummaries");
            var weeklySummary = JsonConvert.DeserializeObject<WeeklySummary>(json);
            if (weeklySummary.ReportDateTime < GetWeeklyForecastTime())
            {
                json = await datastore.GetInfoDataAsync("weeklySummaries" + GetForecastTime().ToString("yyyyMMddTHH"));
                await datastore.SetInfoDataAsnc("weeklySummaries", json, DateTime.UtcNow);
                await Utils.WriteLog("週間予報サマリーの更新ができていませんでした。");
                return;
            }
            await Utils.WriteLog("週間予報サマリーチェック終了");
        }

        private static DateTime GetWeeklyForecastTime()
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var h = now.Hour;
            if (now.Minute > 30)
                h++;
            //11時
            if (h < 2)
                return date.AddHours(-16);
            //17時
            else if (h < 8)
                return date.AddHours(2);
            else
                return date.AddHours(8);
        }
    }
}