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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BetValue = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    Payout = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinBet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxBet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "betting"),
                    Result = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BettingClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_bets_player_id",
                schema: "game",
                table: "bets",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "idx_bets_round_id",
                schema: "game",
                table: "bets",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "idx_bets_status",
                schema: "game",
                table: "bets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_started_at",
                schema: "game",
                table: "rounds",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_status",
                schema: "game",
                table: "rounds",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_rounds_table_id",
                schema: "game",
                table: "rounds",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "idx_tables_game_id",
                schema: "game",
                table: "tables",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "idx_tables_status",
                schema: "game",
                table: "tables",
                column: "Status");
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
