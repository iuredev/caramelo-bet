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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    table_id = table.Column<Guid>(type: "uuid", nullable: true),
                    round_id = table.Column<Guid>(type: "uuid", nullable: true),
                    bet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_events_created_at",
                schema: "history",
                table: "events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_events_event_type",
                schema: "history",
                table: "events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "idx_events_round_id",
                schema: "history",
                table: "events",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_user_id",
                schema: "history",
                table: "events",
                column: "user_id");
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
