using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JmaXmlClient.Data;

namespace JmaXmlClient.Migrations
{
    [DbContext(typeof(ForecastContext))]
    partial class ForecastContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpcw50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpfd50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpfg50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpfw50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpww53", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpww54", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaVpzw50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JmaXmlInfo", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpcw50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpfd50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpfg50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpfw50", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpww53", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpww54", b =>
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

            modelBuilder.Entity("JmaXmlClient.Data.JsonVpzw50", b =>
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
