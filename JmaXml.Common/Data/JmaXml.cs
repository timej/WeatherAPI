using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JmaXml.Common.Data
{
    public class JmaXml
    {
        [Column("task", Order = 0)]
        public string Task { get; set; }
        [Column("id", Order = 1)]
        public int Id { get; set; }
        [Column("forecast", Order = 2)]
        public string Forecast { get; set; }
        [Column("update", Order = 3, TypeName = "timestamptz")]
        public DateTime Update { get; set; }
    }
}
