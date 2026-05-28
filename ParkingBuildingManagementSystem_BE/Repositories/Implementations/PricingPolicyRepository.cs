using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class PricingPolicyRepository(AppDbContext context) : IPricingPolicyRepository
{
    public async Task<PricingPolicy?> GetActiveByVehicleTypeAndFloorAsync(int vehicleTypeId, int floorId)
    {
        var today = DateOnly.FromDateTime(VnTime.Now);

        // Lấy tất cả policy phù hợp: đúng vehicleType, đúng tầng hoặc áp dụng toàn tòa (FloorId null),
        // đang trong thời hạn hiệu lực. OrderBy ưu tiên policy tầng cụ thể (HasValue = true → DESC).
        return await context.PricingPolicies
            .AsNoTracking()
            .Where(p => p.VehicleTypeId == vehicleTypeId
                     && (p.FloorId == null || p.FloorId == floorId)
                     && p.EffectiveFrom <= today
                     && (p.EffectiveTo == null || p.EffectiveTo >= today))
            .OrderByDescending(p => p.FloorId.HasValue)
            .FirstOrDefaultAsync();
    }
}
