using System;
using System.Collections.Generic;

namespace ParkingBuildingManagementSystem_BE.Models;

public partial class PricingPolicy
{
    public int Id { get; set; }

    public int VehicleTypeId { get; set; }

    public int? FloorId { get; set; }

    public decimal PricePerHour { get; set; }

    public decimal DepositAmount { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Floor? Floor { get; set; }

    public virtual VehicleType VehicleType { get; set; } = null!;
}
