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

            modelBuilder.Entity<JmaXmlInfo>()
                .ToTable("jma_xml_info");
            modelBuilder.Entity<JmaXmlRegular>()
                .ToTable("jma_xml_regular");
            modelBuilder.Entity<JmaXmlExtra>()
                .ToTable("jma_xml_extra");
        }
    }
}
