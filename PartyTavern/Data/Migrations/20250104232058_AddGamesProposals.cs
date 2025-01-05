using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyTavern.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGamesProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameProposals_SubmittedByUserId",
                table: "GameProposals",
                column: "SubmittedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameProposals");
        }
    }
}
