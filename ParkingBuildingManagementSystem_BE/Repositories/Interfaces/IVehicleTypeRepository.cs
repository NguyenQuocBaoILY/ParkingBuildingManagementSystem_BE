using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IVehicleTypeRepository
{
    Task<IEnumerable<VehicleType>> GetAllAsync();
    Task<VehicleType?> GetByIdAsync(int id);
    Task<VehicleType?> GetByNameAsync(string name);
    Task AddAsync(VehicleType vehicleType);
    void Update(VehicleType vehicleType);
    void Delete(VehicleType vehicleType);
    Task<bool> IsInUseAsync(int id);
    Task SaveChangesAsync();
}
