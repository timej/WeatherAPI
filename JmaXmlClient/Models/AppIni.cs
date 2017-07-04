using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JmaXmlClient.Models
{
    public class AppIni
    {
        public static string DataPath { get; private set; }
        public static Dictionary<string, int> PublishingOffice { get; private set; }
        public static string ProjectId { get; private set; }
        public static bool IsOutputToPostgreSQL { get; private set; }
        public static bool IsOutputToDatastore { get; private set; }
        public static void Init(string basePath, IConfigurationRoot configuration)
        {
            DataPath = Path.Combine(basePath, "App_Data");
            string path = Path.Combine(DataPath, "logs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            ProjectId = configuration["Google:ProjectId"];
            IsOutputToPostgreSQL = bool.Parse(configuration["Output:PostgreSQL"]);
            IsOutputToDatastore = bool.Parse(configuration["Output:Datastore"]);

            PublishingOffice = new Dictionary<string, int>()
            {
                {"稚内地方気象台", 11}, {"旭川地方気象台", 12}, {"網走地方気象台", 13}, {"釧路地方気象台", 14}, {"帯広測候所", 19}, {"室蘭地方気象台", 15}, {"札幌管区気象台", 16}, {"函館地方気象台", 17},
                {"青森地方気象台", 20}, {"盛岡地方気象台", 30}, {"仙台管区気象台", 40}, {"秋田地方気象台", 50}, {"山形地方気象台", 60}, {"福島地方気象台", 70},
                {"水戸地方気象台", 80}, {"宇都宮地方気象台", 90}, {"前橋地方気象台", 100}, {"熊谷地方気象台", 110}, {"銚子地方気象台", 120}, {"気象庁予報部", 130}, {"横浜地方気象台", 140},
                {"新潟地方気象台", 150}, {"富山地方気象台", 160}, {"金沢地方気象台", 170}, {"福井地方気象台", 180}, {"甲府地方気象台", 190}, {"長野地方気象台", 200},
                {"岐阜地方気象台", 210}, {"静岡地方気象台", 220}, {"名古屋地方気象台", 230}, {"津地方気象台", 240},
                {"彦根地方気象台", 250}, {"京都地方気象台", 260}, {"大阪管区気象台", 270}, {"神戸地方気象台", 280}, {"奈良地方気象台", 290}, {"和歌山地方気象台", 300},
                {"鳥取地方気象台", 310}, {"松江地方気象台", 320}, {"岡山地方気象台", 330}, {"広島地方気象台", 340}, {"下関地方気象台", 350},
                { "徳島地方気象台", 360}, {"高松地方気象台", 370}, {"松山地方気象台", 380}, {"高知地方気象台", 390},
                { "福岡管区気象台", 400}, {"佐賀地方気象台", 410}, {"長崎地方気象台", 420}, {"熊本地方気象台", 430}, {"大分地方気象台", 440}, {"宮崎地方気象台", 450}, {"鹿児島地方気象台", 460}, {"名瀬測候所", 469},
                {"沖縄気象台", 471}, {"南大東島地方気象台", 472}, {"宮古島地方気象台", 473}, {"石垣島地方気象台", 474}
            };
        }
    }
}

