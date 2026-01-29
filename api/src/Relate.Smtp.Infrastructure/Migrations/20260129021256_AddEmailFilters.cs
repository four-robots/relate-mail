using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relate.Smtp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    FromAddressContains = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SubjectContains = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BodyContains = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    HasAttachments = table.Column<bool>(type: "INTEGER", nullable: true),
                    MarkAsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    AssignLabelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Delete = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastAppliedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TimesApplied = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailFilters_Labels_AssignLabelId",
                        column: x => x.AssignLabelId,
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmailFilters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailFilters_AssignLabelId",
                table: "EmailFilters",
                column: "AssignLabelId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFilters_UserId_Priority",
                table: "EmailFilters",
                columns: new[] { "UserId", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailFilters");
        }
    }
}
