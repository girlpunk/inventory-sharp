using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class initial_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemLabels_ForeignServerId",
                table: "ItemLabels");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ForeignServerId_Identifier",
                table: "ItemLabels",
                columns: new[] { "ForeignServerId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForeignServers_Namespace",
                table: "ForeignServers",
                column: "Namespace",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemLabels_ForeignServerId_Identifier",
                table: "ItemLabels");

            migrationBuilder.DropIndex(
                name: "IX_ForeignServers_Namespace",
                table: "ForeignServers");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ForeignServerId",
                table: "ItemLabels",
                column: "ForeignServerId");
        }
    }
}
