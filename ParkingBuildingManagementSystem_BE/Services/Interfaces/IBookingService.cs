using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(int userId, BookingRequest request);
    Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int userId);
    Task<BookingResponse> GetByIdAsync(int userId, int bookingId);
    Task CancelAsync(int userId, int bookingId);
}
