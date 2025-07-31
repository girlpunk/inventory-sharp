using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class initial_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabelScans_Scanners_ScannerId",
                table: "LabelScans");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScannerId",
                table: "LabelScans",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_LabelScans_Scanners_ScannerId",
                table: "LabelScans",
                column: "ScannerId",
                principalTable: "Scanners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabelScans_Scanners_ScannerId",
                table: "LabelScans");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScannerId",
                table: "LabelScans",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LabelScans_Scanners_ScannerId",
                table: "LabelScans",
                column: "ScannerId",
                principalTable: "Scanners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
