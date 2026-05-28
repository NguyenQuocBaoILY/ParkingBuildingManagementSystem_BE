using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);

    /// <summary>
    /// Kiểm tra xe này có đang có booking chưa kết thúc không (pending_payment hoặc confirmed).
    /// 1 xe vật lý không thể đỗ 2 nơi cùng lúc.
    /// </summary>
    Task<bool> ExistsActiveByVehicleIdAsync(int vehicleId);
    Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
    Task<Booking?> GetByIdWithDetailsAsync(int id);
    Task AddAsync(Booking booking);
    Task SaveChangesAsync();
}
