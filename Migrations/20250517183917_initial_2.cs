using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class initial_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ForeignServers_ForeignServerId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ItemScans");

            migrationBuilder.DropIndex(
                name: "IX_Items_ForeignServerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ForeignServerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Namespace",
                table: "ItemLabels");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Scanners",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Scanners",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentItemId",
                table: "Scanners",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForeignServerId",
                table: "ItemLabels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabelScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LabelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScannerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Scanned = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScanType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelScans_ItemLabels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "ItemLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelScans_Scanners_ScannerId",
                        column: x => x.ScannerId,
                        principalTable: "Scanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ForeignServerId",
                table: "ItemLabels",
                column: "ForeignServerId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelScans_LabelId",
                table: "LabelScans",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelScans_ScannerId",
                table: "LabelScans",
                column: "ScannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemLabels_ForeignServers_ForeignServerId",
                table: "ItemLabels",
                column: "ForeignServerId",
                principalTable: "ForeignServers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemLabels_ForeignServers_ForeignServerId",
                table: "ItemLabels");

            migrationBuilder.DropTable(
                name: "LabelScans");

            migrationBuilder.DropIndex(
                name: "IX_ItemLabels_ForeignServerId",
                table: "ItemLabels");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Scanners");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Scanners");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                table: "Scanners");

            migrationBuilder.DropColumn(
                name: "ForeignServerId",
                table: "ItemLabels");

            migrationBuilder.AddColumn<Guid>(
                name: "ForeignServerId",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Namespace",
                table: "ItemLabels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ItemScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemTagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    ScanType = table.Column<int>(type: "INTEGER", nullable: false),
                    Scanned = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScannerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemScans_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ForeignServerId",
                table: "Items",
                column: "ForeignServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemScans_ItemId",
                table: "ItemScans",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ForeignServers_ForeignServerId",
                table: "Items",
                column: "ForeignServerId",
                principalTable: "ForeignServers",
                principalColumn: "Id");
        }
    }
}
