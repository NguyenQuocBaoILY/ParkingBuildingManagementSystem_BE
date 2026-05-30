using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Incident> Incidents { get; set; }

    public virtual DbSet<ParkingSession> ParkingSessions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PricingPolicy> PricingPolicies { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83F003B2CE4");

            entity.ToTable("activity_logs");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_logs_user_date").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetTable)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("target_table");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_logs_user");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookings__3213E83FEBE65F1B");

            entity.ToTable("bookings");

            entity.HasIndex(e => e.QrCode, "UQ__bookings__E2FB88893F98E923").IsUnique().HasFilter("[qr_code] IS NOT NULL");

            entity.HasIndex(e => new { e.CheckinDeadline, e.Status }, "idx_bookings_expiry").HasFilter("([status]='confirmed')");

            entity.HasIndex(e => new { e.FloorId, e.Status, e.CheckinDeadline }, "idx_bookings_floor_confirmed").HasFilter("([status]='confirmed')");

            entity.HasIndex(e => new { e.UserId, e.Status, e.CreatedAt }, "idx_bookings_user_status").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(300)
                .HasColumnName("cancel_reason");
            entity.Property(e => e.CheckinDeadline).HasColumnName("checkin_deadline");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.DepositPaid)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("deposit_paid");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.QrCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("qr_code");
            entity.Property(e => e.ScheduledCheckin).HasColumnName("scheduled_checkin");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending_payment")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.Floor).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FloorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_floor");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_user");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_vehicle");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_vtype");
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__floors__3213E83F7A91A04E");

            entity.ToTable("floors");

            entity.HasIndex(e => e.FloorNumber, "UQ__floors__4AE77A4B24253A70").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.FloorNumber).HasColumnName("floor_number");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.TotalSlots).HasColumnName("total_slots");

            entity.HasMany(d => d.VehicleTypes).WithMany(p => p.Floors)
                .UsingEntity<Dictionary<string, object>>(
                    "FloorVehicleType",
                    r => r.HasOne<VehicleType>().WithMany()
                        .HasForeignKey("VehicleTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_fvt_type"),
                    l => l.HasOne<Floor>().WithMany()
                        .HasForeignKey("FloorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_fvt_floor"),
                    j =>
                    {
                        j.HasKey("FloorId", "VehicleTypeId").HasName("pk_floor_vehicle_types");
                        j.ToTable("floor_vehicle_types");
                        j.IndexerProperty<int>("FloorId").HasColumnName("floor_id");
                        j.IndexerProperty<int>("VehicleTypeId").HasColumnName("vehicle_type_id");
                    });
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__incident__3213E83FE972DBA6");

            entity.ToTable("incidents");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "idx_incidents_open").HasFilter("([status] IN ('open', 'in_progress'))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HandledBy).HasColumnName("handled_by");
            entity.Property(e => e.IncidentType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("incident_type");
            entity.Property(e => e.ReportedBy).HasColumnName("reported_by");
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("open")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.HandledByNavigation).WithMany(p => p.IncidentHandledByNavigations)
                .HasForeignKey(d => d.HandledBy)
                .HasConstraintName("fk_incidents_handler");

            entity.HasOne(d => d.ReportedByNavigation).WithMany(p => p.IncidentReportedByNavigations)
                .HasForeignKey(d => d.ReportedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_incidents_reporter");

            entity.HasOne(d => d.Session).WithMany(p => p.Incidents)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("fk_incidents_session");
        });

        modelBuilder.Entity<ParkingSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__parking___3213E83F2153E77F");

            entity.ToTable("parking_sessions");

            entity.HasIndex(e => new { e.FloorId, e.Status }, "idx_sessions_floor_active").HasFilter("([status]='active')");

            entity.HasIndex(e => new { e.LicensePlateSnapshot, e.Status }, "idx_sessions_plate");

            entity.HasIndex(e => new { e.FloorId, e.VehicleTypeId, e.ActualCheckin, e.ActualCheckout }, "idx_sessions_report");

            entity.HasIndex(e => new { e.VehicleId, e.ActualCheckin }, "idx_sessions_vehicle")
                .IsDescending(false, true)
                .HasFilter("([vehicle_id] IS NOT NULL)");

            entity.HasIndex(e => e.LicensePlateSnapshot, "uq_active_session_plate")
                .IsUnique()
                .HasFilter("([status]='active')");

            entity.HasIndex(e => e.VehicleId, "uq_active_session_vehicle")
                .IsUnique()
                .HasFilter("([status]='active' AND [vehicle_id] IS NOT NULL)");

            entity.HasIndex(e => e.BookingId, "uq_session_per_booking")
                .IsUnique()
                .HasFilter("([booking_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualCheckin)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("actual_checkin");
            entity.Property(e => e.ActualCheckout).HasColumnName("actual_checkout");
            entity.Property(e => e.AppliedPricePerHour)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("applied_price_per_hour");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CheckinFaceImg)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("checkin_face_img");
            entity.Property(e => e.CheckinVehicleImg)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("checkin_vehicle_img");
            entity.Property(e => e.CheckoutFaceImg)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("checkout_face_img");
            entity.Property(e => e.CheckoutVehicleImg)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("checkout_vehicle_img");
            entity.Property(e => e.EntryGate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("entry_gate");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.LicensePlateSnapshot)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("license_plate_snapshot");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.SessionType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("walkin")
                .HasColumnName("session_type");
            entity.Property(e => e.StaffCheckinId).HasColumnName("staff_checkin_id");
            entity.Property(e => e.StaffCheckoutId).HasColumnName("staff_checkout_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.Booking).WithOne(p => p.ParkingSession)
                .HasForeignKey<ParkingSession>(d => d.BookingId)
                .HasConstraintName("fk_session_booking");

            entity.HasOne(d => d.Floor).WithMany(p => p.ParkingSessions)
                .HasForeignKey(d => d.FloorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_session_floor");

            entity.HasOne(d => d.StaffCheckin).WithMany(p => p.ParkingSessionStaffCheckins)
                .HasForeignKey(d => d.StaffCheckinId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_session_staff_in");

            entity.HasOne(d => d.StaffCheckout).WithMany(p => p.ParkingSessionStaffCheckouts)
                .HasForeignKey(d => d.StaffCheckoutId)
                .HasConstraintName("fk_session_staff_out");

            entity.HasOne(d => d.Vehicle).WithOne(p => p.ParkingSession)
                .HasForeignKey<ParkingSession>(d => d.VehicleId)
                .HasConstraintName("fk_session_vehicle");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.ParkingSessions)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_session_vtype");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83FCFC56C14");

            entity.ToTable("payments");

            entity.HasIndex(e => new { e.BookingId, e.Status }, "idx_payments_booking");

            entity.HasIndex(e => new { e.Status, e.PaidAt, e.PaymentType }, "idx_payments_revenue").HasFilter("([status]='completed')");

            entity.HasIndex(e => new { e.SessionId, e.PaymentType, e.Status }, "idx_payments_session");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("cash")
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("payment_type");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TransactionRef)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("transaction_ref");
            entity.Property(e => e.ViolationId).HasColumnName("violation_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_payments_booking");

            entity.HasOne(d => d.Session).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("fk_payments_session");

            entity.HasOne(d => d.Violation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ViolationId)
                .HasConstraintName("fk_payments_violation");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__policies__3213E83F25269192");

            entity.ToTable("policies");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PolicyType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("policy_type");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Policies)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("fk_policies_updater");
        });

        modelBuilder.Entity<PricingPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pricing___3213E83FDF3B755A");

            entity.ToTable("pricing_policies");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepositAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("deposit_amount");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.PricePerHour)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price_per_hour");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PricingPolicies)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pricing_creator");

            entity.HasOne(d => d.Floor).WithMany(p => p.PricingPolicies)
                .HasForeignKey(d => d.FloorId)
                .HasConstraintName("fk_pricing_floor");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.PricingPolicies)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pricing_type");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reviews__3213E83F306D2C90");

            entity.ToTable("reviews");

            entity.HasIndex(e => new { e.UserId, e.SessionId }, "uq_review_per_session").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.ManagerReply).HasColumnName("manager_reply");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.RepliedAt).HasColumnName("replied_at");
            entity.Property(e => e.RepliedBy).HasColumnName("replied_by");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.RepliedByNavigation).WithMany(p => p.ReviewRepliedByNavigations)
                .HasForeignKey(d => d.RepliedBy)
                .HasConstraintName("fk_reviews_replier");

            entity.HasOne(d => d.Session).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_session");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FFA080EC5");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164D9C2C462").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_email_verified");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("updated_at");
        });



        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicles__3213E83FA00C3C32");

            entity.ToTable("vehicles");

            entity.HasIndex(e => e.LicensePlate, "UQ__vehicles__F72CD56E063A8FF0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("license_plate");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.User).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vehicles_user");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vehicles_type");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FD65AFB26");

            entity.ToTable("vehicle_types");

            entity.HasIndex(e => e.Name, "UQ__vehicle___72E12F1B89295807").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__violatio__3213E83F712F549B");

            entity.ToTable("violations");

            entity.HasIndex(e => new { e.SessionId, e.IsPaid }, "idx_violations_unpaid").HasFilter("([is_paid]=(0))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.FineAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("fine_amount");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("recorded_at");
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.ViolationType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("violation_type");

            entity.HasOne(d => d.RecordedByNavigation).WithMany(p => p.Violations)
                .HasForeignKey(d => d.RecordedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violations_recorder");

            entity.HasOne(d => d.Session).WithMany(p => p.Violations)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violations_session");
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__email_verification_tokens__id");

            entity.ToTable("email_verification_tokens");

            // Tra cứu token theo giá trị — phải unique vì token là định danh duy nhất
            entity.HasIndex(e => e.Token, "UQ__evt__token").IsUnique();

            // Index hỗ trợ truy vấn: "lấy token hợp lệ của user theo loại"
            entity.HasIndex(e => new { e.UserId, e.TokenType, e.IsUsed }, "idx_evt_user_type_used");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.TokenType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("token_type");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(DATEADD(hour, 7, GETUTCDATE()))")
                .HasColumnName("created_at");

            entity.HasOne(d => d.User)
                .WithMany(p => p.EmailVerificationTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_evt_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
