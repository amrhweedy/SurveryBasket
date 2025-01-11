using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisabledColumnInTableUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01943aba-fd25-77ae-ae13-1782e31ac3a5",
                columns: new[] { "IsDisabled", "PasswordHash" },
                values: new object[] { false, "AQAAAAIAAYagAAAAEEkZjCjr58jQZZQcNNHC8pGi2EF9/g1AnN/YIobUVeCtP5zXiUd03VVgtkcYIihqqg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01943aba-fd25-77ae-ae13-1782e31ac3a5",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIQhih843cnsmQD8c+mLOYAN/1SyjsA6U7IfCGaCHA+G6BTQnzJ4JNwkvtJtvP0SXA==");
        }
    }
}
