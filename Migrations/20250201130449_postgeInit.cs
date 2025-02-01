using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionAPI.Migrations
{
    /// <inheritdoc />
    public partial class postgeInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    transaction_id = table.Column<string>(type: "varchar", nullable: false),
                    name = table.Column<string>(type: "varchar", nullable: false),
                    email = table.Column<string>(type: "varchar", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    client_location = table.Column<string>(type: "varchar", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.transaction_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
