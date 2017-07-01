using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JmaXmlServer.Models
{
    public class AppConst
    {
        public static string ClientPath { get; private set; }
        public static string ProjectId { get; private set; }

        public static void Ini(IConfigurationRoot configuration)
        {
            ClientPath = configuration.GetValue<string>("ClientPath");
            ProjectId = configuration.GetSection("Google")["ProjectId"];
        }

        
    }
}
