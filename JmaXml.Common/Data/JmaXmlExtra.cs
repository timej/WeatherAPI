using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JmaXml.Common.Data
{
    public class JmaXmlExtra
    {
        [Column("id", Order = 0, TypeName = "serial")]
        public int Id { get; set; }
        [Column("created", Order = 1, TypeName = "timestamptz")]
        public DateTime Created { get; set; }
        [Column("feeds", Order = 2)]
        public string Feeds { get; set; }
    }
}
