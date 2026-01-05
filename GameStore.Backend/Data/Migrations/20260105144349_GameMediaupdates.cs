using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class GameMediaupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "GameMedia",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "GameMedia");
        }
    }
}
