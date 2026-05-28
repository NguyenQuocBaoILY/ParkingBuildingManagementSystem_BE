using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingBuildingManagementSystem_BE.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "recorded_at",
                table: "violations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "reviews",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "pricing_policies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "policies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "payments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "actual_checkin",
                table: "parking_sessions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "incidents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "incidents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "email_verification_tokens",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "activity_logs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "recorded_at",
                table: "violations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "reviews",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "pricing_policies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "policies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "payments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "actual_checkin",
                table: "parking_sessions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "incidents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "incidents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "email_verification_tokens",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "activity_logs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(hour, 7, GETUTCDATE()))");
        }
    }
}
