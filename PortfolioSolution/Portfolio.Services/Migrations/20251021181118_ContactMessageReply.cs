using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Services.Migrations
{
    /// <inheritdoc />
    public partial class ContactMessageReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessageReplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReplyMessage = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ReplyToEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RepliedById = table.Column<int>(type: "int", nullable: false),
                    RepliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessageReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactMessageReplies_ContactMessages_ContactMessageId",
                        column: x => x.ContactMessageId,
                        principalTable: "ContactMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactMessageReplies_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactMessageReplies_Users_RepliedById",
                        column: x => x.RepliedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessageReplies_ContactMessageId",
                table: "ContactMessageReplies",
                column: "ContactMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessageReplies_DeletedById",
                table: "ContactMessageReplies",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessageReplies_RepliedById",
                table: "ContactMessageReplies",
                column: "RepliedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessageReplies");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 5, 22, 40, 20, 883, DateTimeKind.Unspecified).AddTicks(545), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 5, 22, 40, 20, 883, DateTimeKind.Unspecified).AddTicks(547), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 5, 22, 40, 20, 883, DateTimeKind.Unspecified).AddTicks(548), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 5, 22, 40, 20, 883, DateTimeKind.Unspecified).AddTicks(542), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 5, 22, 40, 20, 883, DateTimeKind.Unspecified).AddTicks(61), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
