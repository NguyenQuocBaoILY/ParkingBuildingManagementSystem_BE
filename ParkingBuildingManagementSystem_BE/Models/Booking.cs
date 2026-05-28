using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int VehicleId { get; set; }

    public int FloorId { get; set; }

    public int VehicleTypeId { get; set; }

    public DateTime ScheduledCheckin { get; set; }

    public DateTime CheckinDeadline { get; set; }

    public decimal DepositPaid { get; set; }

    public string Status { get; set; } = null!;

    public string? QrCode { get; set; }

    public string? CancelReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Floor Floor { get; set; } = null!;

    public virtual ParkingSession? ParkingSession { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
