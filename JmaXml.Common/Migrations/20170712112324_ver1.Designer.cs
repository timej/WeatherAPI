using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JmaXml.Common.Data;

namespace JmaXml.Common.Migrations
{
    [DbContext(typeof(ForecastContext))]
    [Migration("20170712112324_ver1")]
    partial class ver1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("JmaXml.Common.Data.JmaJson", b =>
                {
                    b.Property<string>("Task")
                        .HasColumnName("task");

                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Task", "Id");

                    b.ToTable("jma_json");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaXml", b =>
                {
                    b.Property<string>("Task")
                        .HasColumnName("task");

                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Task", "Id");

                    b.ToTable("jma_xml");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaXmlExtra", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Feeds")
                        .HasColumnName("feeds");

                    b.HasKey("Id");

                    b.ToTable("jma_xml_extra");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaXmlInfo", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Info")
                        .HasColumnName("info");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_xml_info");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaXmlRegular", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Feeds")
                        .HasColumnName("feeds");

                    b.HasKey("Id");

                    b.ToTable("jma_xml_regular");
                });
        }
    }
}
