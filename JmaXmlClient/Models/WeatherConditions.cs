using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    class WeatherConditions: JmaForecastBase
    {
        [JsonProperty("code")]
        public int AreaCode { get; set; }
        [JsonProperty("name")]
        public string AreaName { get; set; }
        [JsonProperty("hl")]
        public string Headline { get; set; }
        [JsonProperty("nt")]
        public string Notice { get; set; }
        [JsonProperty("cm")]
        public string Comment { get; set; }

        internal WeatherConditions(string xml, int pref) : base(xml, pref)
        {
            try
            {
                var headline = xe.Descendants(Utils.XmlnsJmxIb + "Headline").First();
                Headline = headline.Element(Utils.XmlnsJmxIb + "Text").Value;

                var targetArea = xe.Descendants(Utils.XmlnsJmxEx + "TargetArea").FirstOrDefault();
                if (targetArea != null)
                {
                    AreaCode = int.Parse(targetArea.Element(Utils.XmlnsJmxEx + "Code").Value);
                    AreaName = targetArea.Element(Utils.XmlnsJmxEx + "Name").Value;
                }

                var notice = xe.Descendants(Utils.XmlnsJmxEx + "Notice").First();
                Comment = notice.Value;

                var comment = xe.Descendants(Utils.XmlnsJmxEx + "Comment").First();
                Comment = comment.Element(Utils.XmlnsJmxEx + "Text").Value;
            }
            catch(Exception e1)
            {
                Utils.WriteLog($"WeatherConditionsでエラー {e1.Message}").GetAwaiter().GetResult();
            }
        }

    }
}
