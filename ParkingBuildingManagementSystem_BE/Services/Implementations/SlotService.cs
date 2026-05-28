using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class SlotService(
    IVehicleTypeRepository vehicleTypeRepository,
    IFloorRepository floorRepository) : ISlotService
{
    public async Task<SlotAvailabilityResponse> GetAvailabilityAsync(int vehicleTypeId)
    {
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(vehicleTypeId)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        var floors = (await floorRepository.GetAvailableByVehicleTypeAsync(vehicleTypeId)).ToList();

        return new SlotAvailabilityResponse
        {
            VehicleTypeId       = vehicleType.Id,
            VehicleTypeName     = vehicleType.Name,
            TotalAvailableSlots = floors.Sum(f => f.AvailableSlots),
            Floors              = floors
        };
    }
}
