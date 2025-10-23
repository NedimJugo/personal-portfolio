using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalMessageIdToReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalMessageId",
                table: "ContactMessageReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 16, 34, 12, 499, DateTimeKind.Unspecified).AddTicks(9452), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 16, 34, 12, 499, DateTimeKind.Unspecified).AddTicks(9455), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 16, 34, 12, 499, DateTimeKind.Unspecified).AddTicks(9458), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 16, 34, 12, 499, DateTimeKind.Unspecified).AddTicks(9445), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 16, 34, 12, 499, DateTimeKind.Unspecified).AddTicks(8208), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalMessageId",
                table: "ContactMessageReplies");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 21, 18, 11, 17, 737, DateTimeKind.Unspecified).AddTicks(1569), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 21, 18, 11, 17, 737, DateTimeKind.Unspecified).AddTicks(1572), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 21, 18, 11, 17, 737, DateTimeKind.Unspecified).AddTicks(1574), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 21, 18, 11, 17, 737, DateTimeKind.Unspecified).AddTicks(1560), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 21, 18, 11, 17, 737, DateTimeKind.Unspecified).AddTicks(856), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
