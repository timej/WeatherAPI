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
            if (bool.Parse(Configuration["Output:PostgreSQL"]))
            {
                services.AddDbContext<ForecastContext>(options => options.UseNpgsql(Configuration.GetConnectionString("ForecastConnection")));
            }
        }

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            if (bool.Parse(Configuration["Output:PostgreSQL"]))
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
                JmaXmlRegularTask.RegularAsync(_forecastContext).GetAwaiter().GetResult();
                JmaXmlRegularTask2.RegularAsync(_forecastContext).GetAwaiter().GetResult();
            }
            //警報・注意報等
            else if (args.Contains("-e"))
                JmaXmlExtraTask.ExtraAsync(_forecastContext).GetAwaiter().GetResult();
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
            var datastore = new JmaDatastore2(AppIni.ProjectId);
            var data = await datastore.GetJmaUpdateAsync("vpfd50");

            //int office = 360;
            //var datastore = new JmaDatastore(AppIni.ProjectId, "JmaVpfg50");
            //string xml = await datastore.GetJmaXmlAsync(office);
            //var forecast = new JmaForecast(xml, office);

#if !DEBUG
            await Utils.WriteLog("終了");
#endif
        }

        static async Task DeleteData()
        {
            var datastore = new JmaDatastore(AppIni.ProjectId, "JmaXmlExtra");
            await datastore.DeleteFeedsAsync("JmaXmlExtra");
            var datastore2 = new JmaDatastore(AppIni.ProjectId, "JmaXmlRegular");
            await datastore2.DeleteFeedsAsync("JmaXmlRegular");
        }

        //天気予報XMLデータから一括して天気予報Jsonデータを作成
        static async Task MakeJsonData()
        {
            await Utils.WriteLog("天気予報Jsonデータ一括作成開始");

            if(AppIni.IsOutputToPostgreSQL)
            {
                string sql = "INSERT INTO json_vpfg50 (id, forecast, update) VALUES(@id, @forecast, @update) " +
                    "ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

                var options = new DbContextOptionsBuilder<ForecastContext>()
                    .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));
                
                using (var context2 = new ForecastContext(options.Options))
                {

                    foreach (var data in _forecastContext.JmaVpfg50)
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

            if (AppIni.IsOutputToDatastore)
            {
                //入力側
                var datastore = new JmaDatastore(AppIni.ProjectId, "JmaVpfg50");
                var results = await datastore.GetAllJmaXmlAsync("JmaVpfg50");

                //保存側
                var datasore2 = new JmaDatastore(AppIni.ProjectId, "JsonVpfg50");
                var entityList = new List<Google.Cloud.Datastore.V1.Entity>();

                foreach (var entity in results.Entities)
                {
                    string xml = entity.Properties["forecast"].StringValue;
                    var dt = entity.Properties["update"].TimestampValue;
                    int id = (int)entity.Key.Path[0].Id;
                    JmaForecast jmaForecast = new JmaForecast(xml, id);
                    entityList.Add(datasore2.SetEntity(id, JsonConvert.SerializeObject(jmaForecast), dt.ToDateTime()));
                }
                await datasore2.UpsertForecastAsync(entityList);
            }
            await Utils.WriteLog("天気予報Jsonデータ一括作成終了");
        }

        //週間天気予報XMLデータから一括して週間天気予報Jsonデータを作成
        static async Task MakeWeeklyJsonData()
        {
            await Utils.WriteLog("週間予報Jsonデータ一括作成開始");

            if (AppIni.IsOutputToPostgreSQL)
            {
                string sql = "INSERT INTO json_vpfw50 (id, forecast, update) VALUES(@id, @forecast, @update) " +
                    "ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

                var options = new DbContextOptionsBuilder<ForecastContext> ()
                    .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));
                
                using (var context2 = new ForecastContext(options.Options))
                {
                    foreach (var data in _forecastContext.JmaVpfw50)
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

            if (AppIni.IsOutputToDatastore)
            {
                //入力側
                var datastore = new JmaDatastore(AppIni.ProjectId, "JmaVpfw50");
                var results = await datastore.GetAllJmaXmlAsync("JmaVpfw50");

                //保存側
                var datasore2 = new JmaDatastore(AppIni.ProjectId, "JsonVpfw50");
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
            }
            await Utils.WriteLog("週間予報Jsonデータ一括作成終了");
        }

        //府県天気概況XMLデータから一括して府県天気概況Jsonデータを作成
        static async Task WeatherConditionJsonData()
        {
            await Utils.WriteLog("天気概況Jsonデータ一括作成開始");

            if (AppIni.IsOutputToPostgreSQL)
            {
                PgConditionXmlToJson(_forecastContext.JmaVpfd50, "json_vpfd50");
                PgConditionXmlToJson(_forecastContext.JmaVpcw50, "json_vpcw50");
                PgConditionXmlToJson(_forecastContext.JmaVpzw50, "json_vpzw50");
            }

            if (AppIni.IsOutputToDatastore)
            {
                await ConditionXmlToJson("JmaVpfd50", "JsonVpfd50");
                await ConditionXmlToJson("JmaVpcw50", "JsonVpcw50");
                await ConditionXmlToJson("JmaVpzw50", "JsonVpzw50");
            }

            await Utils.WriteLog("天気概況Jsonデータ一括作成終了");
        }

        static void PgConditionXmlToJson<T>(DbSet<T> dbset, string JsonTable) where T: ForecastTable
        {
            string sql = $"INSERT INTO {JsonTable} (id, forecast, update) VALUES(@id, @forecast, @update) " +
                "ON CONFLICT(id) DO UPDATE SET forecast = EXCLUDED.forecast, update = EXCLUDED.update;";

            var options = new DbContextOptionsBuilder<ForecastContext>()
                .UseNpgsql(Configuration.GetConnectionString("ForecastConnection"));
          
            using (var context2 = new ForecastContext(options.Options))
            {

                foreach (var data in dbset)
                {
                    WeatherConditions conditions = new WeatherConditions(data.Forecast, data.Id);
                    string json = JsonConvert.SerializeObject(conditions);

                    NpgsqlParameter id = new NpgsqlParameter("id", data.Id);
                    NpgsqlParameter update = new NpgsqlParameter("update", data.Update);
                    NpgsqlParameter forecast = new NpgsqlParameter("forecast", json);

                    context2.Database.ExecuteSqlCommand(sql, id, forecast, update);
                }
            }
        }

        static async Task ConditionXmlToJson(string kindXml, string kindJson)
        {
            //入力側
            var datastore1 = new JmaDatastore(AppIni.ProjectId, kindXml);
            //保存側
            var datastore2 = new JmaDatastore(AppIni.ProjectId, kindJson);
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