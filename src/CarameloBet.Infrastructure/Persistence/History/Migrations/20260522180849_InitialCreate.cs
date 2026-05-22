using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarameloBet.Infrastructure.Persistence.History.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "history");

            migrationBuilder.CreateTable(
                name: "events",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TableId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: true),
                    BetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_events_created_at",
                schema: "history",
                table: "events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_events_event_type",
                schema: "history",
                table: "events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "idx_events_round_id",
                schema: "history",
                table: "events",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "idx_events_user_id",
                schema: "history",
                table: "events",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "history");
        }
    }
}
