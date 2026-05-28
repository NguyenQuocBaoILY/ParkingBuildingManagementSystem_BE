using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class ParkingSession
{
    public int Id { get; set; }

    public int? BookingId { get; set; }

    public int FloorId { get; set; }

    public int? VehicleId { get; set; }

    public int VehicleTypeId { get; set; }

    public string LicensePlateSnapshot { get; set; } = null!;

    public decimal AppliedPricePerHour { get; set; }

    public int StaffCheckinId { get; set; }

    public int? StaffCheckoutId { get; set; }

    public DateTime ActualCheckin { get; set; }

    public DateTime? ActualCheckout { get; set; }

    public string EntryGate { get; set; } = null!;

    public string SessionType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? CheckinVehicleImg { get; set; }

    public string? CheckinFaceImg { get; set; }

    public string? CheckoutVehicleImg { get; set; }

    public string? CheckoutFaceImg { get; set; }

    public string? Notes { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Floor Floor { get; set; } = null!;

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User StaffCheckin { get; set; } = null!;

    public virtual User? StaffCheckout { get; set; }

    public virtual Vehicle? Vehicle { get; set; }

    public virtual VehicleType VehicleType { get; set; } = null!;

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
