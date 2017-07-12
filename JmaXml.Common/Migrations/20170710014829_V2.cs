using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JmaXml.Common.Migrations
{
    public partial class V2 : Migration
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jma_json");

            migrationBuilder.DropTable(
                name: "jma_xml");
        }
    }
}
