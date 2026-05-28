using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class PaymentRepository(AppDbContext context) : IPaymentRepository
{
    public async Task AddAsync(Payment payment) =>
        await context.Payments.AddAsync(payment);

    public async Task<Payment?> GetByIdAsync(int paymentId) =>
        await context.Payments.FindAsync(paymentId);

    public async Task<Payment?> GetPendingByBookingIdAsync(int bookingId) =>
        await context.Payments
            .Where(p => p.BookingId == bookingId && p.Status == "pending")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
