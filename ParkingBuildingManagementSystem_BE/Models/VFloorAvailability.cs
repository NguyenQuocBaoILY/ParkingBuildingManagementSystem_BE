using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class VFloorAvailability
{
    public int FloorId { get; set; }

    public string FloorName { get; set; } = null!;

    public int TotalSlots { get; set; }

    public int? OccupiedCount { get; set; }

    public int? ReservedCount { get; set; }

    public int? AvailableCount { get; set; }
}
