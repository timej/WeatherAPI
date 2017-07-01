using System;
using System.Collections.Generic;
using System.Text;

namespace JmaXmlClient.Models
{
    class ThreeHourlyAreaData
    {
        public List<string> Weather { get; set; }
        public List<string> WindDirection { get; set; }
        public List<int> WindSpeed { get; set; }
    }
}
