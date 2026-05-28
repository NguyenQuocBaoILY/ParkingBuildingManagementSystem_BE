using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Incident
{
    public int Id { get; set; }

    public int ReportedBy { get; set; }

    public int? SessionId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string IncidentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? HandledBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? Resolution { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? HandledByNavigation { get; set; }

    public virtual User ReportedByNavigation { get; set; } = null!;

    public virtual ParkingSession? Session { get; set; }
}
