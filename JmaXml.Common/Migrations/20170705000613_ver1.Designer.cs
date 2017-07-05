using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JmaXml.Common.Data;

namespace JmaXml.Common.Migrations
{
    [DbContext(typeof(ForecastContext))]
    [Migration("20170705000613_ver1")]
    partial class ver1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpcw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpcw50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpfd50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpfd50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpfg50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpfg50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpfw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpfw50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpww53", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpww53");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpww54", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpww54");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JmaVpzw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("jma_vpzw50");
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

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpcw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpcw50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpfd50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpfd50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpfg50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpfg50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpfw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpfw50");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpww53", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpww53");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpww54", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpww54");
                });

            modelBuilder.Entity("JmaXml.Common.Data.JsonVpzw50", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Forecast")
                        .HasColumnName("forecast");

                    b.Property<DateTime>("Update")
                        .HasColumnName("update")
                        .HasColumnType("timestamptz");

                    b.HasKey("Id");

                    b.ToTable("json_vpzw50");
                });
        }
    }
}
