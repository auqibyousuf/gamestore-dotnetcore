using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class BasketModelupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Games_GameID",
                table: "BasketItems");

            migrationBuilder.RenameColumn(
                name: "GameID",
                table: "BasketItems",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_BasketItems_GameID",
                table: "BasketItems",
                newName: "IX_BasketItems_GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Games_GameId",
                table: "BasketItems",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Games_GameId",
                table: "BasketItems");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "BasketItems",
                newName: "GameID");

            migrationBuilder.RenameIndex(
                name: "IX_BasketItems_GameId",
                table: "BasketItems",
                newName: "IX_BasketItems_GameID");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Games_GameID",
                table: "BasketItems",
                column: "GameID",
                principalTable: "Games",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
