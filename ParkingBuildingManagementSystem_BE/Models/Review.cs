using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SessionId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public string? ManagerReply { get; set; }

    public int? RepliedBy { get; set; }

    public DateTime? RepliedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? RepliedByNavigation { get; set; }

    public virtual ParkingSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
