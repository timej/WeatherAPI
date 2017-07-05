using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JmaXml.Common.Migrations
{
    public partial class ver1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jma_vpcw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpcw50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpfd50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpfd50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpfg50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpfg50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpfw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpfw50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpww53",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpww53", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpww54",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpww54", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_vpzw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_vpzw50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_xml_extra",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    feeds = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_xml_extra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_xml_info",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    info = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_xml_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jma_xml_regular",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    feeds = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_xml_regular", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpcw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpcw50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpfd50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpfd50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpfg50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpfg50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpfw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpfw50", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpww53",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpww53", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpww54",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpww54", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "json_vpzw50",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_json_vpzw50", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jma_vpcw50");

            migrationBuilder.DropTable(
                name: "jma_vpfd50");

            migrationBuilder.DropTable(
                name: "jma_vpfg50");

            migrationBuilder.DropTable(
                name: "jma_vpfw50");

            migrationBuilder.DropTable(
                name: "jma_vpww53");

            migrationBuilder.DropTable(
                name: "jma_vpww54");

            migrationBuilder.DropTable(
                name: "jma_vpzw50");

            migrationBuilder.DropTable(
                name: "jma_xml_extra");

            migrationBuilder.DropTable(
                name: "jma_xml_info");

            migrationBuilder.DropTable(
                name: "jma_xml_regular");

            migrationBuilder.DropTable(
                name: "json_vpcw50");

            migrationBuilder.DropTable(
                name: "json_vpfd50");

            migrationBuilder.DropTable(
                name: "json_vpfg50");

            migrationBuilder.DropTable(
                name: "json_vpfw50");

            migrationBuilder.DropTable(
                name: "json_vpww53");

            migrationBuilder.DropTable(
                name: "json_vpww54");

            migrationBuilder.DropTable(
                name: "json_vpzw50");
        }
    }
}
