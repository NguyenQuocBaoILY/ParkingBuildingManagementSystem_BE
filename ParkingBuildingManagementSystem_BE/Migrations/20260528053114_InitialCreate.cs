using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingBuildingManagementSystem_BE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Leaf tables (no FK dependencies) ─────────────────────────────
            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    floor_number = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    total_slots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__floors__3213E83F7A91A04E", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicle___3213E83FD65AFB26", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_email_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83FFA080EC5", x => x.id);
                });

            // ── 2. Tables depending only on leaf tables ───────────────────────────
            migrationBuilder.CreateTable(
                name: "floor_vehicle_types",
                columns: table => new
                {
                    floor_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_floor_vehicle_types", x => new { x.floor_id, x.vehicle_type_id });
                    table.ForeignKey(
                        name: "fk_fvt_floor",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_fvt_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    license_plate = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicles__3213E83FA00C3C32", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicles_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_vehicles_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    target_table = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    target_id = table.Column<int>(type: "int", nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__activity__3213E83F003B2CE4", x => x.id);
                    table.ForeignKey(
                        name: "fk_logs_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    token_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__email_verification_tokens__id", x => x.id);
                    table.ForeignKey(
                        name: "fk_evt_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    policy_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__policies__3213E83F25269192", x => x.id);
                    table.ForeignKey(
                        name: "fk_policies_updater",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            // ── 3. bookings (depends on floors, users, vehicles, vehicle_types) ──
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    floor_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    scheduled_checkin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deposit_paid = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "pending_payment"),
                    qr_code = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__bookings__3213E83FEBE65F1B", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_floor",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_vehicle",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_vtype",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pricing_policies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    floor_id = table.Column<int>(type: "int", nullable: true),
                    price_per_hour = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__pricing___3213E83FDF3B755A", x => x.id);
                    table.ForeignKey(
                        name: "fk_pricing_creator",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_pricing_floor",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_pricing_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            // ── 4. parking_sessions (depends on bookings, floors, users, vehicles, vehicle_types) ──
            migrationBuilder.CreateTable(
                name: "parking_sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    floor_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    license_plate_snapshot = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    session_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "walkin"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "active"),
                    entry_gate = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    staff_checkin_id = table.Column<int>(type: "int", nullable: false),
                    staff_checkout_id = table.Column<int>(type: "int", nullable: true),
                    actual_checkin = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    actual_checkout = table.Column<DateTime>(type: "datetime2", nullable: true),
                    applied_price_per_hour = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    checkin_face_img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    checkin_vehicle_img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    checkout_face_img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    checkout_vehicle_img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__parking___3213E83F2153E77F", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_floor",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_staff_in",
                        column: x => x.staff_checkin_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_staff_out",
                        column: x => x.staff_checkout_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_vehicle",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_vtype",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            // ── 5. Tables depending on parking_sessions ──────────────────────────
            migrationBuilder.CreateTable(
                name: "violations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    violation_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    fine_amount = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    is_paid = table.Column<bool>(type: "bit", nullable: false),
                    recorded_by = table.Column<int>(type: "int", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__violatio__3213E83F712F549B", x => x.id);
                    table.ForeignKey(
                        name: "fk_violations_recorder",
                        column: x => x.recorded_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_violations_session",
                        column: x => x.session_id,
                        principalTable: "parking_sessions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    incident_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "open"),
                    reported_by = table.Column<int>(type: "int", nullable: false),
                    handled_by = table.Column<int>(type: "int", nullable: true),
                    resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__incident__3213E83FE972DBA6", x => x.id);
                    table.ForeignKey(
                        name: "fk_incidents_handler",
                        column: x => x.handled_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_incidents_reporter",
                        column: x => x.reported_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_incidents_session",
                        column: x => x.session_id,
                        principalTable: "parking_sessions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    manager_reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    replied_by = table.Column<int>(type: "int", nullable: true),
                    replied_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reviews__3213E83F306D2C90", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviews_replier",
                        column: x => x.replied_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_session",
                        column: x => x.session_id,
                        principalTable: "parking_sessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            // ── 6. payments (depends on bookings, parking_sessions, violations) ──
            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    session_id = table.Column<int>(type: "int", nullable: true),
                    violation_id = table.Column<int>(type: "int", nullable: true),
                    payment_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    payment_method = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "cash"),
                    amount = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "pending"),
                    transaction_ref = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payments__3213E83FCFC56C14", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payments_session",
                        column: x => x.session_id,
                        principalTable: "parking_sessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payments_violation",
                        column: x => x.violation_id,
                        principalTable: "violations",
                        principalColumn: "id");
                });

            // ── 7. CHECK constraint on payments.payment_method ───────────────────
            migrationBuilder.Sql(
                "ALTER TABLE [payments] ADD CONSTRAINT [chk_payments_method] CHECK ([payment_method] IN ('cash', 'payos'));");

            // ── 8. Indexes ────────────────────────────────────────────────────────

            // activity_logs
            migrationBuilder.CreateIndex(
                name: "idx_logs_user_date",
                table: "activity_logs",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            // bookings
            migrationBuilder.CreateIndex(
                name: "UQ__bookings__E2FB88893F98E923",
                table: "bookings",
                column: "qr_code",
                unique: true,
                filter: "[qr_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_bookings_expiry",
                table: "bookings",
                columns: new[] { "checkin_deadline", "status" },
                filter: "([status]='confirmed')");

            migrationBuilder.CreateIndex(
                name: "idx_bookings_floor_confirmed",
                table: "bookings",
                columns: new[] { "floor_id", "status", "checkin_deadline" },
                filter: "([status]='confirmed')");

            migrationBuilder.CreateIndex(
                name: "idx_bookings_user_status",
                table: "bookings",
                columns: new[] { "user_id", "status", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_vehicle_id",
                table: "bookings",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_vehicle_type_id",
                table: "bookings",
                column: "vehicle_type_id");

            // email_verification_tokens
            migrationBuilder.CreateIndex(
                name: "UQ__evt__token",
                table: "email_verification_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_evt_user_type_used",
                table: "email_verification_tokens",
                columns: new[] { "user_id", "token_type", "is_used" });

            // floors
            migrationBuilder.CreateIndex(
                name: "UQ__floors__4AE77A4B24253A70",
                table: "floors",
                column: "floor_number",
                unique: true);

            // floor_vehicle_types
            migrationBuilder.CreateIndex(
                name: "IX_floor_vehicle_types_vehicle_type_id",
                table: "floor_vehicle_types",
                column: "vehicle_type_id");

            // incidents
            migrationBuilder.CreateIndex(
                name: "IX_incidents_handled_by",
                table: "incidents",
                column: "handled_by");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_reported_by",
                table: "incidents",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_session_id",
                table: "incidents",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_open",
                table: "incidents",
                columns: new[] { "status", "created_at" },
                filter: "([status] IN ('open', 'in_progress'))");

            // parking_sessions
            migrationBuilder.CreateIndex(
                name: "IX_parking_sessions_staff_checkin_id",
                table: "parking_sessions",
                column: "staff_checkin_id");

            migrationBuilder.CreateIndex(
                name: "IX_parking_sessions_staff_checkout_id",
                table: "parking_sessions",
                column: "staff_checkout_id");

            migrationBuilder.CreateIndex(
                name: "IX_parking_sessions_vehicle_type_id",
                table: "parking_sessions",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_floor_active",
                table: "parking_sessions",
                columns: new[] { "floor_id", "status" },
                filter: "([status]='active')");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_plate",
                table: "parking_sessions",
                columns: new[] { "license_plate_snapshot", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_sessions_report",
                table: "parking_sessions",
                columns: new[] { "floor_id", "vehicle_type_id", "actual_checkin", "actual_checkout" });

            migrationBuilder.CreateIndex(
                name: "idx_sessions_vehicle",
                table: "parking_sessions",
                columns: new[] { "vehicle_id", "actual_checkin" },
                descending: new[] { false, true },
                filter: "([vehicle_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "uq_active_session_plate",
                table: "parking_sessions",
                column: "license_plate_snapshot",
                unique: true,
                filter: "([status]='active')");

            migrationBuilder.CreateIndex(
                name: "uq_active_session_vehicle",
                table: "parking_sessions",
                column: "vehicle_id",
                unique: true,
                filter: "([status]='active' AND [vehicle_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "uq_session_per_booking",
                table: "parking_sessions",
                column: "booking_id",
                unique: true,
                filter: "([booking_id] IS NOT NULL)");

            // payments
            migrationBuilder.CreateIndex(
                name: "IX_payments_violation_id",
                table: "payments",
                column: "violation_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_booking",
                table: "payments",
                columns: new[] { "booking_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_payments_revenue",
                table: "payments",
                columns: new[] { "status", "paid_at", "payment_type" },
                filter: "([status]='completed')");

            migrationBuilder.CreateIndex(
                name: "idx_payments_session",
                table: "payments",
                columns: new[] { "session_id", "payment_type", "status" });

            // policies
            migrationBuilder.CreateIndex(
                name: "IX_policies_updated_by",
                table: "policies",
                column: "updated_by");

            // pricing_policies
            migrationBuilder.CreateIndex(
                name: "IX_pricing_policies_created_by",
                table: "pricing_policies",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_policies_floor_id",
                table: "pricing_policies",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_policies_vehicle_type_id",
                table: "pricing_policies",
                column: "vehicle_type_id");

            // reviews
            migrationBuilder.CreateIndex(
                name: "IX_reviews_replied_by",
                table: "reviews",
                column: "replied_by");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_session_id",
                table: "reviews",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "uq_review_per_session",
                table: "reviews",
                columns: new[] { "user_id", "session_id" },
                unique: true);

            // users
            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E6164D9C2C462",
                table: "users",
                column: "email",
                unique: true);

            // vehicles
            migrationBuilder.CreateIndex(
                name: "IX_vehicles_user_id",
                table: "vehicles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_vehicle_type_id",
                table: "vehicles",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "UQ__vehicles__F72CD56E063A8FF0",
                table: "vehicles",
                column: "license_plate",
                unique: true);

            // vehicle_types
            migrationBuilder.CreateIndex(
                name: "UQ__vehicle___72E12F1B89295807",
                table: "vehicle_types",
                column: "name",
                unique: true);

            // violations
            migrationBuilder.CreateIndex(
                name: "IX_violations_recorded_by",
                table: "violations",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "idx_violations_unpaid",
                table: "violations",
                columns: new[] { "session_id", "is_paid" },
                filter: "([is_paid]=(0))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "floor_vehicle_types");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "policies");

            migrationBuilder.DropTable(
                name: "pricing_policies");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "violations");

            migrationBuilder.DropTable(
                name: "parking_sessions");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_types");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

