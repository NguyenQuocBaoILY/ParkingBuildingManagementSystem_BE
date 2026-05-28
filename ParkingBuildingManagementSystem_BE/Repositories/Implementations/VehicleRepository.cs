using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class VehicleRepository(AppDbContext context) : IVehicleRepository
{
    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate) =>
        await context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

    public async Task AddAsync(Vehicle vehicle) =>
        await context.Vehicles.AddAsync(vehicle);

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
