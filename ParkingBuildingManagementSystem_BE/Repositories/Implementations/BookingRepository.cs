using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private static readonly string[] ActiveStatuses = ["pending_payment", "confirmed"];

    public async Task<Booking?> GetByIdAsync(int id) =>
        await context.Bookings.FindAsync(id);

    public async Task<bool> ExistsActiveByVehicleIdAsync(int vehicleId) =>
        await context.Bookings.AnyAsync(b =>
            b.VehicleId == vehicleId &&
            ActiveStatuses.Contains(b.Status));

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId) =>
        await context.Bookings
            .Include(b => b.Floor)
            .Include(b => b.Vehicle)
            .Include(b => b.VehicleType)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Booking?> GetByIdWithDetailsAsync(int id) =>
        await context.Bookings
            .Include(b => b.Floor)
            .Include(b => b.Vehicle)
            .Include(b => b.VehicleType)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddAsync(Booking booking) =>
        await context.Bookings.AddAsync(booking);

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
