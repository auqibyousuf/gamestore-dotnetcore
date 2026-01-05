using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameMediaPrimaryFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "GameMedia",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "GameMedia");
        }
    }
}
