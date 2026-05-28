using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
    Task AddAsync(Vehicle vehicle);
    Task SaveChangesAsync();
}
