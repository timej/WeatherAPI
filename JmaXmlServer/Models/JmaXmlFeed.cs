using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JmaXmlServer.Models
{
    public class JmaXmlFeed
    {
        public string Task { get; set; }
        public string Author { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Link { get; set; }
    }
}
