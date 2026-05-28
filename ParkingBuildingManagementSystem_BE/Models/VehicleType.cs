using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class VehicleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    public virtual ICollection<PricingPolicy> PricingPolicies { get; set; } = new List<PricingPolicy>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
