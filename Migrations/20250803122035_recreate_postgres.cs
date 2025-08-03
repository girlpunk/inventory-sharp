using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventorySharp.Migrations
{
    /// <inheritdoc />
    public partial class recreate_postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Events",
                columns: table => new
                {
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DelayUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Events", x => x.Uuid);
                });

            migrationBuilder.CreateTable(
                name: "_Operations",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    HostId = table.Column<string>(type: "text", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommandJson = table.Column<string>(type: "text", nullable: false),
                    ItemsJson = table.Column<string>(type: "text", nullable: true),
                    NestedOperations = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Operations", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "ForeignServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namespace = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Uri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeignServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Scanners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScannerType = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    ParentItemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LabelType = table.Column<int>(type: "integer", nullable: false),
                    ForeignServerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemLabels_ForeignServers_ForeignServerId",
                        column: x => x.ForeignServerId,
                        principalTable: "ForeignServers",
                        principalColumn: "Id");
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Uploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
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
                name: "ItemTags",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LabelScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScannerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Scanned = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScanType = table.Column<int>(type: "integer", nullable: false)
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
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ForeignServers_Namespace",
                table: "ForeignServers",
                column: "Namespace",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ForeignServerId_Identifier",
                table: "ItemLabels",
                columns: new[] { "ForeignServerId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemLabels_ItemId",
                table: "ItemLabels",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId",
                table: "ItemPhotos",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentId",
                table: "Items",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelScans_LabelId",
                table: "LabelScans",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelScans_ScannerId",
                table: "LabelScans",
                column: "ScannerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Events");

            migrationBuilder.DropTable(
                name: "_Operations");

            migrationBuilder.DropTable(
                name: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "ItemTags");

            migrationBuilder.DropTable(
                name: "LabelScans");

            migrationBuilder.DropTable(
                name: "ItemLabels");

            migrationBuilder.DropTable(
                name: "Scanners");

            migrationBuilder.DropTable(
                name: "ForeignServers");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
