using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IVehicleTypeService
{
    Task<IEnumerable<VehicleTypeResponse>> GetAllAsync();
    Task<VehicleTypeResponse> GetByIdAsync(int id);
    Task<VehicleTypeResponse> CreateAsync(VehicleTypeRequest request);
    Task<VehicleTypeResponse> UpdateAsync(int id, VehicleTypeRequest request);
    Task DeleteAsync(int id);
}
