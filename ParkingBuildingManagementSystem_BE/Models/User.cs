using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Incident> IncidentHandledByNavigations { get; set; } = new List<Incident>();

    public virtual ICollection<Incident> IncidentReportedByNavigations { get; set; } = new List<Incident>();

    public virtual ICollection<ParkingSession> ParkingSessionStaffCheckins { get; set; } = new List<ParkingSession>();

    public virtual ICollection<ParkingSession> ParkingSessionStaffCheckouts { get; set; } = new List<ParkingSession>();

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<PricingPolicy> PricingPolicies { get; set; } = new List<PricingPolicy>();

    public virtual ICollection<Review> ReviewRepliedByNavigations { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewUsers { get; set; } = new List<Review>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
