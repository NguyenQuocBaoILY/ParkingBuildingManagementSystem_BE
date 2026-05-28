using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class VehicleTypeRepository(AppDbContext context) : IVehicleTypeRepository
{
    public async Task<IEnumerable<VehicleType>> GetAllAsync() =>
        await context.VehicleTypes.AsNoTracking().ToListAsync();

    public async Task<VehicleType?> GetByIdAsync(int id) =>
        await context.VehicleTypes.FirstOrDefaultAsync(v => v.Id == id);

    public async Task<VehicleType?> GetByNameAsync(string name) =>
        await context.VehicleTypes.FirstOrDefaultAsync(v => v.Name == name);

    public async Task AddAsync(VehicleType vehicleType) =>
        await context.VehicleTypes.AddAsync(vehicleType);

    public void Update(VehicleType vehicleType) =>
        context.VehicleTypes.Update(vehicleType);

    public void Delete(VehicleType vehicleType) =>
        context.VehicleTypes.Remove(vehicleType);

    // Kiểm tra xem VehicleType có đang được dùng bởi bất kỳ thực thể liên quan nào không
    public async Task<bool> IsInUseAsync(int id) =>
        await context.Vehicles.AnyAsync(v => v.VehicleTypeId == id) ||
        await context.Bookings.AnyAsync(b => b.VehicleTypeId == id) ||
        await context.ParkingSessions.AnyAsync(s => s.VehicleTypeId == id) ||
        await context.PricingPolicies.AnyAsync(p => p.VehicleTypeId == id);

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
