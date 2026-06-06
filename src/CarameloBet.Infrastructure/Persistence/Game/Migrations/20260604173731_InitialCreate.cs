using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarameloBet.Infrastructure.Persistence.Game.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "game");

            migrationBuilder.CreateTable(
                name: "bets",
                schema: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bet_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bet_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    payout = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                schema: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    min_bet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    max_bet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                schema: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "betting"),
                    result = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    betting_closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rounds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                schema: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    max_players = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tables", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_bets_player_id",
                schema: "game",
                table: "bets",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "idx_bets_round_id",
                schema: "game",
                table: "bets",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "idx_bets_status",
                schema: "game",
                table: "bets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_started_at",
                schema: "game",
                table: "rounds",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_status",
                schema: "game",
                table: "rounds",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_table_id",
                schema: "game",
                table: "rounds",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "idx_tables_game_id",
                schema: "game",
                table: "tables",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "idx_tables_status",
                schema: "game",
                table: "tables",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bets",
                schema: "game");

            migrationBuilder.DropTable(
                name: "games",
                schema: "game");

            migrationBuilder.DropTable(
                name: "rounds",
                schema: "game");

            migrationBuilder.DropTable(
                name: "tables",
                schema: "game");
        }
    }
}
