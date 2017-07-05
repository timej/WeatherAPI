using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using JmaXmlServer.Models;
using JmaXml.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace JmaXmlServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new ThreadIdEnricher())
                .MinimumLevel.Warning()
                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "logs", "log-{Date}.txt"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(x => x.Properties["topic"].ToString() == "\"challenge\"")
                    .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "logs", "challenge-{Date}.txt")))
                .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ForecastContext>(options => options.UseNpgsql(Configuration.GetConnectionString("ForecastConnection")));

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ForecastContext forecastContext)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            AppConst.Ini(Configuration);

            //PostgreSQLデータベースの自動作成
            if (AppConst.IsOutputToPostgreSQL)
                forecastContext.Database.Migrate();

        }
    }
}
