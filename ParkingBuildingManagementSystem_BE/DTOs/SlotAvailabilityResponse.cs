namespace ParkingBuildingManagementSystem_BE.DTOs;

public class SlotAvailabilityResponse
{
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public int TotalAvailableSlots { get; set; }
    public List<FloorSlotDto> Floors { get; set; } = [];
}

public class FloorSlotDto
{
    public int FloorId { get; set; }
    public string FloorName { get; set; } = null!;
    public int FloorNumber { get; set; }
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int ReservedSlots { get; set; }
    public int AvailableSlots { get; set; }
    public bool HasAvailability => AvailableSlots > 0;
}
