using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailData",
                table: "Media",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Media",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 15, 0, 5, 11, 14, DateTimeKind.Unspecified).AddTicks(6199), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 15, 0, 5, 11, 14, DateTimeKind.Unspecified).AddTicks(6201), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 15, 0, 5, 11, 14, DateTimeKind.Unspecified).AddTicks(6202), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 15, 0, 5, 11, 14, DateTimeKind.Unspecified).AddTicks(6196), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 8, 15, 0, 5, 11, 14, DateTimeKind.Unspecified).AddTicks(5685), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailData",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Media");

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
    }
}
