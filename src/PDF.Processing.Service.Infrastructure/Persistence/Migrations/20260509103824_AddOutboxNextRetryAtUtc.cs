using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDF.Processing.Service.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxNextRetryAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_retry_at_utc",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_retry_at_utc",
                table: "outbox_messages");
        }
    }
}
