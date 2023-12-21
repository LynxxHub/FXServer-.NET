using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace lxEF.Server.Migrations
{
    public partial class CharactersUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    CitizenID = table.Column<string>(type: "varchar(127)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FirstName = table.Column<string>(type: "longtext", nullable: true),
                    Gender = table.Column<string>(type: "longtext", nullable: true),
                    IsDrunk = table.Column<bool>(type: "bit", nullable: false),
                    IsHigh = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "longtext", nullable: true),
                    Nationality = table.Column<string>(type: "longtext", nullable: true),
                    Ped = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(127)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.CitizenID);
                    table.ForeignKey(
                        name: "FK_Characters_DBUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DBUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
