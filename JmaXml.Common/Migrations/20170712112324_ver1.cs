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
                name: "jma_json",
                columns: table => new
                {
                    task = table.Column<string>(nullable: false),
                    id = table.Column<int>(nullable: false),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_json", x => new { x.task, x.id });
                });

            migrationBuilder.CreateTable(
                name: "jma_xml",
                columns: table => new
                {
                    task = table.Column<string>(nullable: false),
                    id = table.Column<int>(nullable: false),
                    forecast = table.Column<string>(nullable: true),
                    update = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jma_xml", x => new { x.task, x.id });
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jma_json");

            migrationBuilder.DropTable(
                name: "jma_xml");

            migrationBuilder.DropTable(
                name: "jma_xml_extra");

            migrationBuilder.DropTable(
                name: "jma_xml_info");

            migrationBuilder.DropTable(
                name: "jma_xml_regular");
        }
    }
}
