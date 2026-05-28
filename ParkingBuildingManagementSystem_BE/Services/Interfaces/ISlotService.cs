using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface ISlotService
{
    Task<SlotAvailabilityResponse> GetAvailabilityAsync(int vehicleTypeId);
}
