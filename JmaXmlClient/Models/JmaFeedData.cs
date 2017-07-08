using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    class JmaFeedData
    {
        public int Id { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Link { get; set; }
    }
}
