using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTimezoneColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_timezone",
                table: "Transactions",
                type: "varchar",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_timezone",
                table: "Transactions");
        }
    }
}
