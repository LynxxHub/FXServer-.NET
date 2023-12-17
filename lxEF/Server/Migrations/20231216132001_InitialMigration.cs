using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace lxEF.Server.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(127)", nullable: false),
                    IP = table.Column<string>(type: "longtext", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsAuthenticated = table.Column<bool>(type: "bit", nullable: false),
                    IsBanned = table.Column<bool>(type: "bit", nullable: false),
                    License = table.Column<string>(type: "longtext", nullable: true),
                    SteamID = table.Column<string>(type: "longtext", nullable: true),
                    Username = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBUsers", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBUsers");
        }
    }
}
