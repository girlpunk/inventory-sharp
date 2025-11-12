using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class timezone_aware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Events_DelayUntil",
                table: "_Events");

            migrationBuilder.CreateIndex(
                name: "IX__Events_DelayUntil_State",
                table: "_Events",
                columns: new[] { "DelayUntil", "State" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Events_DelayUntil_State",
                table: "_Events");

            migrationBuilder.CreateIndex(
                name: "IX__Events_DelayUntil",
                table: "_Events",
                column: "DelayUntil");
        }
    }
}
