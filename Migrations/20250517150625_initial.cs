using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForeignServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Namespace = table.Column<string>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeignServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scanners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScannerType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Events",
                columns: table => new
                {
                    Uuid = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DelayUntil = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValueJson = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Events", x => x.Uuid);
                });

            migrationBuilder.CreateTable(
                name: "_Operations",
                columns: table => new
                {
                    Index = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uuid = table.Column<string>(type: "TEXT", nullable: false),
                    HostId = table.Column<string>(type: "TEXT", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CommandJson = table.Column<string>(type: "TEXT", nullable: false),
                    ItemsJson = table.Column<string>(type: "TEXT", nullable: true),
                    NestedOperations = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Operations", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ForeignServerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_ForeignServers_ForeignServerId",
                        column: x => x.ForeignServerId,
                        principalTable: "ForeignServers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    LabelType = table.Column<int>(type: "INTEGER", nullable: false),
                    Namespace = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemLabels_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Uploaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPhotos_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemTagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScannerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Scanned = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScanType = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ItemTags",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTags", x => new { x.ItemId, x.Tag });
                    table.ForeignKey(
                        name: "FK_ItemTags_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ItemId",
                table: "ItemLabels",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId",
                table: "ItemPhotos",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemScans_ItemId",
                table: "ItemScans",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ForeignServerId",
                table: "Items",
                column: "ForeignServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentId",
                table: "Items",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX__Events_DelayUntil",
                table: "_Events",
                column: "DelayUntil");

            migrationBuilder.CreateIndex(
                name: "IX__Events_State_DelayUntil",
                table: "_Events",
                columns: new[] { "State", "DelayUntil" });

            migrationBuilder.CreateIndex(
                name: "IX__Operations_LoggedAt",
                table: "_Operations",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX__Operations_Uuid",
                table: "_Operations",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemLabels");

            migrationBuilder.DropTable(
                name: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "ItemScans");

            migrationBuilder.DropTable(
                name: "ItemTags");

            migrationBuilder.DropTable(
                name: "Scanners");

            migrationBuilder.DropTable(
                name: "_Events");

            migrationBuilder.DropTable(
                name: "_Operations");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ForeignServers");
        }
    }
}
