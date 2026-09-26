using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lash.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityEmailRequestLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identity_email_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_email_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_identity_email_requests_email_hash_operation_requested_at",
                table: "identity_email_requests",
                columns: new[] { "email_hash", "operation", "requested_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_email_requests");
        }
    }
}
