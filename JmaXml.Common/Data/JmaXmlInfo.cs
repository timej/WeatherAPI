using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JmaXml.Common.Data
{
    public class JmaXmlInfo
    {
        [Column("id")]
        public string Id { get; set; }
        [Column("info")]
        public string Info { get; set; }
        [Column("update", TypeName = "timestamptz")]
        public DateTime Update { get; set; }
    }
}
