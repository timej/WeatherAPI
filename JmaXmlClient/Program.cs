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
using Google.Cloud.Datastore.V1;

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
                _forecastContext.Database.Migrate();
            }


            AppIni.Init(_env.ApplicationBasePath, Configuration);

            //Windows のコマンドプロンプトが既定ではSift_JISコードのための対応
#if DEBUG
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            //天気予報等
            if (args.Contains("-r"))
            {
                if(AppIni.IsUseDatastore)
                    JmaDsRegularTask.RegularAsync(_forecastContext).GetAwaiter().GetResult();
                if (AppIni.IsUsePostgreSQL)
                    JmaPgRegularTask.RegularAsync(_forecastContext).GetAwaiter().GetResult();
            }
            //警報・注意報等
            else if (args.Contains("-e"))
            {
                if (AppIni.IsUseDatastore)
                    JmaDsExtraTask.ExtraAsync(_forecastContext).GetAwaiter().GetResult();
                if (AppIni.IsUsePostgreSQL)
                    JmaPgExtraTask.ExtraAsync(_forecastContext).GetAwaiter().GetResult();
            }
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
            var datastore = new JmaDatastore(AppIni.ProjectId);
            string xml = await datastore.GetJmaXmlAsync("vpfd50", office);
            var forecast = new JmaForecast(xml, office);

#if !DEBUG
            await Utils.WriteLog("終了");
#endif
        }

        static async Task DeleteData()
        {
            if (AppIni.IsUsePostgreSQL)
            {
                string sql = $"DELETE FROM public.jma_xml_extra WHERE created < '{DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-ddTHH:mm:ssZ")}'; ";
                //_forecastContext.Database.ExecuteSqlCommand(sql);

                sql = $"DELETE FROM public.jma_xml_regular WHERE created < '{DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-ddTHH:mm:ssZ")}'; ";
                // _forecastContext.Database.ExecuteSqlCommand(sql);
            }

            if (AppIni.IsUseDatastore)
            {
                var datastore = new JmaDatastore(AppIni.ProjectId);
                await datastore.DeleteFeedsAsync(24);
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
                    int id = (int)entity.Key.Path[0].Id;
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
                    int id = (int)entity.Key.Path[0].Id;
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
                int id = (int)entity.Key.Path[0].Id;
                var conditions = new WeatherConditions(xml, id);
                entityList.Add(datastore2.SetEntity("JmaJson", task, id, JsonConvert.SerializeObject(conditions), dt.ToDateTime()));
            }
            await datastore2.UpsertForecastAsync(entityList);
        }
    }
}