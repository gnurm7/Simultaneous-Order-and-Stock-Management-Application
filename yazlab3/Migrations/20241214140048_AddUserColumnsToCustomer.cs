using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yazlab3.Migrations
{
    /// <inheritdoc />
    public partial class AddUserColumnsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Eposta",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KullaniciAdi",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eposta",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KullaniciAdi",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Customers");
        }
    }
}
