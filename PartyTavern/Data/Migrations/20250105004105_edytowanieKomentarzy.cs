using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyTavern.Data.Migrations
{
    /// <inheritdoc />
    public partial class edytowanieKomentarzy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "wasEdited",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wasEdited",
                table: "Comments");
        }
    }
}
