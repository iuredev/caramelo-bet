using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarameloBet.Infrastructure.Persistence.Wallet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wallet");

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Movement = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                schema: "wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "credits"),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_transactions_created_at",
                schema: "wallet",
                table: "transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_idempotency_key",
                schema: "wallet",
                table: "transactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_transactions_wallet_id",
                schema: "wallet",
                table: "transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "idx_wallets_player_id",
                schema: "wallet",
                table: "wallets",
                column: "PlayerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions",
                schema: "wallet");

            migrationBuilder.DropTable(
                name: "wallets",
                schema: "wallet");
        }
    }
}
