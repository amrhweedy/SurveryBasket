using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveyBasket.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "01943aba-fd25-77ae-ae13-1785a9b53ecc", "01943aba-fd25-77ae-ae13-178631426a7e", false, false, "Admin", "ADMIN" },
                    { "01943aba-fd25-77ae-ae13-178762a74ee6", "01943aba-fd25-77ae-ae13-17881a7aaa27", true, false, "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "01943aba-fd25-77ae-ae13-1782e31ac3a5", 0, "01943aba-fd25-77ae-ae13-17843b483b66", "admin@test.com", true, "Amr", "Hweedy", false, null, "ADMIN@TEST.COM", "ADMIN@TEST.COM", "AQAAAAIAAYagAAAAEIQhih843cnsmQD8c+mLOYAN/1SyjsA6U7IfCGaCHA+G6BTQnzJ4JNwkvtJtvP0SXA==", null, false, "01943abafd2577aeae131783b58c03ee", false, "admin@test.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permissions", "polls:read", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 2, "permissions", "polls:add", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 3, "permissions", "polls:update", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 4, "permissions", "polls:delete", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 5, "permissions", "questions:read", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 6, "permissions", "questions:add", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 7, "permissions", "questions:update", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 8, "permissions", "users:read", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 9, "permissions", "users:add", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 10, "permissions", "users:update", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 11, "permissions", "roles:read", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 12, "permissions", "roles:add", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 13, "permissions", "roles:update", "01943aba-fd25-77ae-ae13-1785a9b53ecc" },
                    { 14, "permissions", "results:read", "01943aba-fd25-77ae-ae13-1785a9b53ecc" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "01943aba-fd25-77ae-ae13-1785a9b53ecc", "01943aba-fd25-77ae-ae13-1782e31ac3a5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "01943aba-fd25-77ae-ae13-178762a74ee6");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "01943aba-fd25-77ae-ae13-1785a9b53ecc", "01943aba-fd25-77ae-ae13-1782e31ac3a5" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "01943aba-fd25-77ae-ae13-1785a9b53ecc");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01943aba-fd25-77ae-ae13-1782e31ac3a5");
        }
    }
}
