using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisitorKey",
                table: "PageViews",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlogPostLikes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlogPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LikedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPostLikes_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 3, 14, 20, 36, 316, DateTimeKind.Unspecified).AddTicks(9469), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 3, 14, 20, 36, 316, DateTimeKind.Unspecified).AddTicks(9472), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 3, 14, 20, 36, 316, DateTimeKind.Unspecified).AddTicks(9474), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 3, 14, 20, 36, 316, DateTimeKind.Unspecified).AddTicks(9462), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 10, 3, 14, 20, 36, 316, DateTimeKind.Unspecified).AddTicks(8126), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostLike_BlogPostId_VisitorKey",
                table: "BlogPostLikes",
                columns: new[] { "BlogPostId", "VisitorKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostLikes");

            migrationBuilder.DropColumn(
                name: "VisitorKey",
                table: "PageViews");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "contact_email",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 9, 25, 18, 15, 18, 674, DateTimeKind.Unspecified).AddTicks(3525), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "github_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 9, 25, 18, 15, 18, 674, DateTimeKind.Unspecified).AddTicks(3527), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "linkedin_url",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 9, 25, 18, 15, 18, 674, DateTimeKind.Unspecified).AddTicks(3530), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_description",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 9, 25, 18, 15, 18, 674, DateTimeKind.Unspecified).AddTicks(3520), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "site_title",
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2025, 9, 25, 18, 15, 18, 674, DateTimeKind.Unspecified).AddTicks(2339), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
