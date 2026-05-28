using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class FloorRepository(AppDbContext context) : IFloorRepository
{
    public async Task<IEnumerable<FloorSlotDto>> GetAvailableByVehicleTypeAsync(int vehicleTypeId)
    {
        var now = VnTime.Now;

        // Một query duy nhất: filter floors hỗ trợ vehicleType,
        // đồng thời project sang anonymous type với 2 correlated subquery COUNT.
        // EF Core dịch thành SQL với 2 subquery hiệu quả — không load navigation collection.
        var rows = await context.Floors
            .AsNoTracking()
            .Where(f => f.VehicleTypes.Any(vt => vt.Id == vehicleTypeId))
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.FloorNumber,
                f.TotalSlots,
                OccupiedSlots = f.ParkingSessions
                    .Count(s => s.Status == "active"),
                ReservedSlots = f.Bookings
                    .Count(b => b.Status == "confirmed" && b.CheckinDeadline > now)
            })
            .OrderBy(f => f.FloorNumber)
            .ToListAsync();

        // AvailableSlots tính phía app để tránh biểu thức số học phức tạp trong SQL projection
        return rows.Select(f => new FloorSlotDto
        {
            FloorId       = f.Id,
            FloorName     = f.Name,
            FloorNumber   = f.FloorNumber,
            TotalSlots    = f.TotalSlots,
            OccupiedSlots = f.OccupiedSlots,
            ReservedSlots = f.ReservedSlots,
            AvailableSlots = Math.Max(0, f.TotalSlots - f.OccupiedSlots - f.ReservedSlots)
        });
    }
}
