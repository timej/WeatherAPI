using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JmaXml.Common.Data
{
    public class ForecastContext : DbContext
    {
        public DbSet<JmaXml> JmaXml { get; set; }
        public DbSet<JmaJson> JmaJson { get; set; }

        public DbSet<JmaVpfg50> JmaVpfg50 { get; set; }
        public DbSet<JmaVpfw50> JmaVpfw50 { get; set; }
        public DbSet<JmaVpfd50> JmaVpfd50 { get; set; }
        public DbSet<JmaVpcw50> JmaVpcw50 { get; set; }
        public DbSet<JmaVpzw50> JmaVpzw50 { get; set; }
        public DbSet<JsonVpfg50> JsonVpfg50 { get; set; }
        public DbSet<JsonVpfw50> JsonVpfw50 { get; set; }
        public DbSet<JsonVpfd50> JsonVpfd50 { get; set; }
        public DbSet<JsonVpcw50> JsonVpcw50 { get; set; }
        public DbSet<JsonVpzw50> JsonVpzw50 { get; set; }

        public DbSet<JmaVpww53> JmaVpww53 { get; set; }
        public DbSet<JmaVpww54> JmaVpww54 { get; set; }
        public DbSet<JsonVpww53> JsonVpww53 { get; set; }
        public DbSet<JsonVpww54> JsonVpww54 { get; set; }

        public DbSet<JmaXmlInfo> JmaXmlInfo { get; set; }
        public DbSet<JmaXmlRegular> JmaXmlRegular { get; set; }
        public DbSet<JmaXmlExtra> JmaXmlExtra { get; set; }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Database=forecast;User ID=xxxxx;Password=xxxxxx;");
        }
        */
        
        public ForecastContext(DbContextOptions<ForecastContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JmaXml>()
                .ToTable("jma_xml")
                .HasKey(t => new { t.Task, t.Id });
            modelBuilder.Entity<JmaJson>()
                .ToTable("jma_json")
                .HasKey(t => new { t.Task, t.Id });

            modelBuilder.Entity<JmaVpfg50>()
                .ToTable("jma_vpfg50");
            modelBuilder.Entity<JmaVpfw50>()
                .ToTable("jma_vpfw50");
            modelBuilder.Entity<JmaVpfd50>()
                .ToTable("jma_vpfd50");
            modelBuilder.Entity<JmaVpcw50>()
                .ToTable("jma_vpcw50");
            modelBuilder.Entity<JmaVpzw50>()
                .ToTable("jma_vpzw50");
            modelBuilder.Entity<JsonVpfg50>()
                .ToTable("json_vpfg50");
            modelBuilder.Entity<JsonVpfw50>()
                .ToTable("json_vpfw50");
            modelBuilder.Entity<JsonVpfd50>()
                .ToTable("json_vpfd50");
            modelBuilder.Entity<JsonVpcw50>()
                .ToTable("json_vpcw50");
            modelBuilder.Entity<JsonVpzw50>()
                .ToTable("json_vpzw50");

            modelBuilder.Entity<JmaVpww53>()
                .ToTable("jma_vpww53");
            modelBuilder.Entity<JmaVpww54>()
                .ToTable("jma_vpww54");
            modelBuilder.Entity<JsonVpww53>()
                .ToTable("json_vpww53");
            modelBuilder.Entity<JsonVpww54>()
                .ToTable("json_vpww54");

            modelBuilder.Entity<JmaXmlInfo>()
                .ToTable("jma_xml_info");
            modelBuilder.Entity<JmaXmlRegular>()
                .ToTable("jma_xml_regular");
            modelBuilder.Entity<JmaXmlExtra>()
                .ToTable("jma_xml_extra");
        }
    }

    public class JmaVpfg50: ForecastTable { }
    public class JmaVpfw50 : ForecastTable { }
    public class JmaVpfd50 : ForecastTable { }
    public class JmaVpcw50 : ForecastTable { }
    public class JmaVpzw50 : ForecastTable { }
    public class JsonVpfg50 : ForecastTable { }
    public class JsonVpfw50 : ForecastTable { }
    public class JsonVpfd50 : ForecastTable { }
    public class JsonVpcw50 : ForecastTable { }
    public class JsonVpzw50 : ForecastTable { }

    public class JmaVpww53 : ForecastTable { }
    public class JmaVpww54 : ForecastTable { }
    public class JsonVpww53 : ForecastTable { }
    public class JsonVpww54 : ForecastTable { }



    public class ForecastTable
    {
        [Column("id", Order = 0)]
        public int Id { get; set; }
        [Column("forecast", Order = 1)]
        public string Forecast { get; set; }
        [Column("update", Order = 2, TypeName = "timestamptz")]
        public DateTime Update { get; set; }
    }

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
