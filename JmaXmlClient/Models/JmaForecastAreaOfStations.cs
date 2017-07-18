using JmaXml.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JmaXmlClient.Models
{
    //各観測所がどの気象区に属しているかのコード表
    //「気象庁防災情報XMLフォーマット　技術資料」の個別コード表から作成
    public class JmaForecastAreaOfStation
    {
        public int 観測所コード { get; set; }
        public string 地点 { get; set; }
        public int 気象区コード { get; set; }
    }

    class JmaForecastArea
    {
        private static List<JmaForecastAreaOfStation> _forecastAreaOfStations;

        public static List<JmaForecastAreaOfStation> ForecastAreaOfStations
        {
            get
            {
                if (_forecastAreaOfStations == null)
                {
                    using (StreamReader file = File.OpenText(Path.Combine(AppIni.DataPath, "stations.json")))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        _forecastAreaOfStations = (List<JmaForecastAreaOfStation>)serializer.Deserialize(file, typeof(List<JmaForecastAreaOfStation>));
                    }
                }
                return _forecastAreaOfStations;
            }
        }
    }
}
