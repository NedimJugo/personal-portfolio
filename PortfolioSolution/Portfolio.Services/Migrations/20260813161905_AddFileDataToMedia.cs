using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddFileDataToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Media",
                type: "bytea",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 13, 16, 19, 5, 71, DateTimeKind.Unspecified).AddTicks(6237), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 13, 16, 19, 5, 71, DateTimeKind.Unspecified).AddTicks(6239), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 13, 16, 19, 5, 71, DateTimeKind.Unspecified).AddTicks(6241), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 13, 16, 19, 5, 71, DateTimeKind.Unspecified).AddTicks(6234), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 13, 16, 19, 5, 71, DateTimeKind.Unspecified).AddTicks(5716), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Media");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 31, 19, 41, 18, 80, DateTimeKind.Unspecified).AddTicks(6529), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 31, 19, 41, 18, 80, DateTimeKind.Unspecified).AddTicks(6532), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 31, 19, 41, 18, 80, DateTimeKind.Unspecified).AddTicks(6535), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 31, 19, 41, 18, 80, DateTimeKind.Unspecified).AddTicks(6524), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 31, 19, 41, 18, 80, DateTimeKind.Unspecified).AddTicks(5726), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
