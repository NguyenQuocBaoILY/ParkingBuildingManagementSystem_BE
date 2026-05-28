using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int VehicleTypeId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ParkingSession? ParkingSession { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
