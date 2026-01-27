using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relate.Smtp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FromAddress = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    FromDisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TextBody = table.Column<string>(type: "TEXT", nullable: true),
                    HtmlBody = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OidcSubject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OidcIssuer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailRecipients_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailRecipients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserEmailAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmailAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEmailAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailId",
                table: "EmailAttachments",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_Address",
                table: "EmailRecipients",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_EmailId",
                table: "EmailRecipients",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_UserId_IsRead",
                table: "EmailRecipients",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Emails_MessageId",
                table: "Emails",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_ReceivedAt",
                table: "Emails",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserEmailAddresses_Address",
                table: "UserEmailAddresses",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_UserEmailAddresses_UserId_Address",
                table: "UserEmailAddresses",
                columns: new[] { "UserId", "Address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OidcIssuer_OidcSubject",
                table: "Users",
                columns: new[] { "OidcIssuer", "OidcSubject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmailRecipients");

            migrationBuilder.DropTable(
                name: "UserEmailAddresses");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
