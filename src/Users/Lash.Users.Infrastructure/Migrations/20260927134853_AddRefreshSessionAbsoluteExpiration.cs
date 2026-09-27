using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lash.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshSessionAbsoluteExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "absolute_expires_at",
                table: "refresh_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE refresh_sessions SET absolute_expires_at = expires_at WHERE absolute_expires_at IS NULL");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "absolute_expires_at",
                table: "refresh_sessions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "absolute_expires_at",
                table: "refresh_sessions");
        }
    }
}
