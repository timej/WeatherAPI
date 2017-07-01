using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class JmaTemperature
    {
        [JsonProperty("min")]
        public int? Min { get; set; }
        //朝の最低気温
        [JsonProperty("mmin")]
        public int? MorningMin { get; set; }
        [JsonProperty("max")]
        public int? Max { get; set; }
        //日中の最高気温
        [JsonProperty("dmax")]
        public int? DayTimeMax { get; set; }
    }
}
