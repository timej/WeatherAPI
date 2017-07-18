﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JmaXmlClient.Models
{
    enum JmaForecastTask { vpfd50, vpfg50, vpfw50, vpcw50, vpzw50 };
    enum JmaWarningTask { vpww53, vpww54 };
    public class AppIni
    {
        public static string DataPath { get; private set; }
        public static string ProjectId { get; private set; }
        public static bool IsUsePostgreSQL { get; private set; }
        public static bool IsUseDatastore { get; private set; }
        public static readonly Dictionary<string, int> PublishingOffice = new Dictionary<string, int>()
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

        //週間天気予報に対応する天気予報の辞書
        public static readonly Dictionary<int, (int area, int obsid)[]> ForecastForWeekly = new Dictionary<int, (int area, int obsid)[]>
        {
            {11, new[]{(11000, 11016)}},
            {12, new[]{(12010, 12442)}},
            {13, new[]{(13010, 17341)}},
            {14, new[]{(14020, 19432), (14030, 20432)}},
            {15, new[]{(15010, 21323)}},
            {16, new[]{(16010, 14163)}},
            {17, new[]{(17010, 23232)}},
            {20, new[]{(20010, 31312), (20030, 31602)}},
            {30, new[]{(30010, 33431), (30020, 33472)}},
            {40, new[]{(40010, 34392), (40020, 34461)}},
            {50, new[]{(50010, 32402)}},
            {60, new[]{(60010, 35426)}},
            {70, new[]{(70010, 36126), (70030, 36361)}},
            {80, new[]{(80010, 40201)}},
            {90, new[]{(90010, 41277)}},
            {100, new[]{(100010, 42251), (100020, 42091)}},
            {110, new[]{(110020, 43056)}},
            {120, new[]{(120020, 45147)}},
            {130, new[]{(130010, 44132), (130020, 44172), (130030, 44263), (130040, 44301)}},
            {140, new[]{(140010, 46106)}},
            {150, new[]{(150010, 54232)}},
            {160, new[]{(160010, 55102)}},
            {170, new[]{(170010, 56227)}},
            {180, new[]{(180010, 57066)}},
            {190, new[]{(190010, 49142)}},
            {200, new[]{(200010, 48156), (200020, 48361)}},
            {210, new[]{(210010, 52586), (210020, 52146)}},
            {220, new[]{(220010, 50331)}},
            {230, new[]{(230010, 51106)}},
            {240, new[]{(240010, 53133)}},
            {250, new[]{(250020, 60131), (250010, 60216)}},
            {260, new[]{(260010, 61286), (260020, 61111)}},
            {270, new[]{(270000, 62078)}},
            {280, new[]{(280010, 63518), (280020, 63051)}},
            {290, new[]{(290010, 64036)}},
            {300, new[]{(300010, 65042)}},
            {310, new[]{(310010, 69122)}},
            {320, new[]{(320010, 68132)}},
            {330, new[]{(330010, 66408), (330020, 66186)}},
            {340, new[]{(340010, 67437), (340020, 67116)}},
            {350, new[]{(350010, 81428)}},
            {360, new[]{(360010, 71106)}},
            {370, new[]{(370000, 72086)}},
            {380, new[]{(380010, 73166)}},
            {390, new[]{(390010, 74181)}},
            {400, new[]{(400010, 82182)}},
            {410, new[]{(410010, 85142)}},
            {420, new[]{(420010, 84496), (420030, 84072)}},
            {430, new[]{(430010, 86141)}},
            {440, new[]{(440010, 83216)}},
            {450, new[]{(450010, 87376)}},
            {460, new[]{(460010, 88317), (460040, 88836)}},
            {471, new[]{(471010, 91197)}},
            {472, new[]{(472000, 92011)}},
            {473, new[]{(473000, 93041)}},
            {474, new[]{(474010, 94081)}}
        };

        public static void Init(string basePath, IConfigurationRoot configuration)
        {
            DataPath = Path.Combine(basePath, "App_Data");
            string path = Path.Combine(DataPath, "logs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            ProjectId = configuration["Google:ProjectId"];
            IsUseDatastore = bool.Parse(configuration["Database:Datastore"]);
            IsUsePostgreSQL = bool.Parse(configuration["Database:PostgreSQL"]);
        }
    }
}

