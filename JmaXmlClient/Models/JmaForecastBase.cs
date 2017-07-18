using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    public class JmaForecastBase
    {
        [JsonProperty("pref")]
        public int Prefecture { get; set; }
        [JsonProperty("office")]
        public string PublishingOffice { get; set; }
        [JsonProperty("published")]
        public DateTime PublishedTime { get; set; }
        [JsonProperty("reported")]
        public DateTime ReportDateTime { get; set; }

        protected XElement xe;
        public JmaForecastBase()
        { }
        public JmaForecastBase (string xml, int pref)
        {
            Prefecture = pref;

            try
            {
                xe = XElement.Parse(xml);

                PublishingOffice = xe.Descendants(Utils.XmlnsJmx + "PublishingOffice").First().Value;
                PublishedTime = DateTime.Parse(xe.Descendants(Utils.XmlnsJmx + "DateTime").First().Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                ReportDateTime = DateTime.Parse(xe.Descendants(Utils.XmlnsJmxIb + "ReportDateTime").First().Value);
            }
            catch(Exception e1)
            {
                Utils.WriteLog($"JmaForecastBaseでエラー {e1.Message}").GetAwaiter().GetResult();
            }
        }
    }
}
