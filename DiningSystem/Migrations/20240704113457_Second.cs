using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DiningSystem.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3c85e0b3-1655-40fb-91cd-bbf82cd996a2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "474fae2e-ce71-4049-a2c5-6535238ed274");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9f5f44f2-4735-4532-8061-d65194bd8313");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "32a09255-cd5d-421c-a8ff-9c6a97867f55", null, "customer", "customer" },
                    { "6fb7e2be-e25c-40ec-a496-1c7cae516a53", null, "admin", "admin" },
                    { "d5ee5eb5-0392-43fd-a1ad-d5e3ab6cdd9d", null, "webAdmin", "webAdmin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "32a09255-cd5d-421c-a8ff-9c6a97867f55");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6fb7e2be-e25c-40ec-a496-1c7cae516a53");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d5ee5eb5-0392-43fd-a1ad-d5e3ab6cdd9d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3c85e0b3-1655-40fb-91cd-bbf82cd996a2", null, "webAdmin", "webAdmin" },
                    { "474fae2e-ce71-4049-a2c5-6535238ed274", null, "admin", "admin" },
                    { "9f5f44f2-4735-4532-8061-d65194bd8313", null, "customer", "customer" }
                });
        }
    }
}
