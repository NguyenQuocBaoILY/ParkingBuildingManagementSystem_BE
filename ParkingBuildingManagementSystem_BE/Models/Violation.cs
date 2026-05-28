using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Violation
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string ViolationType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal FineAmount { get; set; }

    public bool IsPaid { get; set; }

    public int RecordedBy { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User RecordedByNavigation { get; set; } = null!;

    public virtual ParkingSession Session { get; set; } = null!;
}
