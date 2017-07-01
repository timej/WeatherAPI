using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class ThreeHourlyPointData
    {
        public int StationCode { get; set; }
        public string StationName { get; set; }
        public List<int> Temperature { get; set; }
    }
}
