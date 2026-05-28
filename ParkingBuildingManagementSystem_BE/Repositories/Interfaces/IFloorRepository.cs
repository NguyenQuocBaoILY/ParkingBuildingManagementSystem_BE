using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IFloorRepository
{
    /// <summary>
    /// Trả về danh sách floor hỗ trợ vehicleTypeId kèm số slot real-time.
    /// OccupiedSlots  = active ParkingSession trên floor đó.
    /// ReservedSlots  = confirmed Booking chưa hết checkin_deadline trên floor đó.
    /// </summary>
    Task<IEnumerable<FloorSlotDto>> GetAvailableByVehicleTypeAsync(int vehicleTypeId);
}
