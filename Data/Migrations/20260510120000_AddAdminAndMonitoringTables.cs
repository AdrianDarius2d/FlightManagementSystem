using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndMonitoringTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── ActivityLogs (REQ-63, REQ-80) ─────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id          = table.Column<int>(type: "int", nullable: false)
                                       .Annotation("SqlServer:Identity", "1, 1"),
                    UserId      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserEmail   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action      = table.Column<string>(type: "nvarchar(50)",  nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType  = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    EntityId    = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    IpAddress   = table.Column<string>(type: "nvarchar(50)",  nullable: true),
                    Timestamp   = table.Column<DateTime>(type: "datetime2",   nullable: false,
                                      defaultValueSql: "GETUTCDATE()"),
                    IsSuccess   = table.Column<bool>(type: "bit",             nullable: false,
                                      defaultValue: true)
                },
                constraints: table => table.PrimaryKey("PK_ActivityLogs", x => x.Id));

            // ── ErrorLogs (REQ-68, REQ-69) ────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id              = table.Column<int>(type: "int", nullable: false)
                                           .Annotation("SqlServer:Identity", "1, 1"),
                    Severity        = table.Column<string>(type: "nvarchar(20)",  nullable: false,
                                          defaultValue: "Error"),
                    Message         = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StackTrace      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestPath     = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    HttpMethod      = table.Column<string>(type: "nvarchar(10)",  nullable: true),
                    UserId          = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress       = table.Column<string>(type: "nvarchar(50)",  nullable: true),
                    Timestamp       = table.Column<DateTime>(type: "datetime2",   nullable: false,
                                          defaultValueSql: "GETUTCDATE()"),
                    IsAcknowledged  = table.Column<bool>(type: "bit",             nullable: false,
                                          defaultValue: false)
                },
                constraints: table => table.PrimaryKey("PK_ErrorLogs", x => x.Id));

            // ── BackupRecords (REQ-54 to REQ-61) ─────────────────────────────
            migrationBuilder.CreateTable(
                name: "BackupRecords",
                columns: table => new
                {
                    Id                 = table.Column<int>(type: "int", nullable: false)
                                              .Annotation("SqlServer:Identity", "1, 1"),
                    FileName           = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    StoragePath        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes      = table.Column<long>(type: "bigint",          nullable: false,
                                             defaultValue: 0L),
                    StartedAt          = table.Column<DateTime>(type: "datetime2",   nullable: false),
                    CompletedAt        = table.Column<DateTime>(type: "datetime2",   nullable: true),
                    Status             = table.Column<string>(type: "nvarchar(20)",  nullable: false,
                                             defaultValue: "InProgress"),
                    Type               = table.Column<string>(type: "nvarchar(20)",  nullable: false,
                                             defaultValue: "Manual"),
                    InitiatedByUserId  = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes              = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrityVerified  = table.Column<bool>(type: "bit",             nullable: false,
                                             defaultValue: false)
                },
                constraints: table => table.PrimaryKey("PK_BackupRecords", x => x.Id));

            // ── SystemSettings (REQ-65, REQ-72) ──────────────────────────────
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key          = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Value        = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName  = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Category     = table.Column<string>(type: "nvarchar(50)",  nullable: false,
                                       defaultValue: "General"),
                    Description  = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputType    = table.Column<string>(type: "nvarchar(20)",  nullable: false,
                                       defaultValue: "text"),
                    LastUpdated  = table.Column<DateTime>(type: "datetime2",   nullable: false,
                                       defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_SystemSettings", x => x.Key));

            // ── Performance index on ActivityLogs.Timestamp ───────────────────
            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Timestamp",
                table: "ActivityLogs",
                column: "Timestamp");

            // ── Performance index on ErrorLogs.IsAcknowledged ─────────────────
            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_IsAcknowledged",
                table: "ErrorLogs",
                column: "IsAcknowledged");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ActivityLogs");
            migrationBuilder.DropTable(name: "ErrorLogs");
            migrationBuilder.DropTable(name: "BackupRecords");
            migrationBuilder.DropTable(name: "SystemSettings");
        }
    }
}
