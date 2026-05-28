using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(int paymentId);
    Task<Payment?> GetPendingByBookingIdAsync(int bookingId);
    Task SaveChangesAsync();
}
