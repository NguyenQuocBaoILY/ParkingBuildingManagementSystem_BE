using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Floor
{
    public int Id { get; set; }

    public int FloorNumber { get; set; }

    public string Name { get; set; } = null!;

    public int TotalSlots { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    public virtual ICollection<PricingPolicy> PricingPolicies { get; set; } = new List<PricingPolicy>();

    public virtual ICollection<VehicleType> VehicleTypes { get; set; } = new List<VehicleType>();
}
