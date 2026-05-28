using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int? SessionId { get; set; }

    public int? BookingId { get; set; }

    public int? ViolationId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentType { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TransactionRef { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ParkingSession? Session { get; set; }

    public virtual Violation? Violation { get; set; }
}
