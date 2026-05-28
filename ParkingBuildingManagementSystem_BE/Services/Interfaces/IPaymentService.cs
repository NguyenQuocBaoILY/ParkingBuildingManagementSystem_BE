using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Gọi PayOS API tạo checkout URL cho lượt đặt chỗ. orderCode = payment.Id.
    /// </summary>
    Task<string> CreatePaymentLinkAsync(Booking booking, Payment payment);

    /// <summary>
    /// Xử lý webhook từ PayOS: xác thực chữ ký HMAC, cập nhật Payment và Booking.
    /// </summary>
    Task HandleWebhookAsync(string rawBody);

    /// <summary>
    /// Tạo PayOS link mới cho booking đang pending_payment (ví dụ link cũ hết hạn).
    /// Hủy payment cũ, tạo payment mới, trả về checkout URL mới.
    /// </summary>
    Task<string> RetryPaymentLinkAsync(int bookingId, int userId);
}
