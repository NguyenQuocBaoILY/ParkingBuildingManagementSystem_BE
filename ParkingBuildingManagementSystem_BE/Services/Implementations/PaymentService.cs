using System.Text.Json;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;
using ParkingBuildingManagementSystem_BE.Settings;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class PaymentService(
    PayOSClient payOsClient,
    IPaymentRepository paymentRepository,
    IBookingRepository bookingRepository,
    IOptions<PayOsSettings> payOsOptions) : IPaymentService
{
    private readonly PayOsSettings _settings = payOsOptions.Value;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> CreatePaymentLinkAsync(Booking booking, Payment payment)
    {
        // Dùng Unix timestamp (ms) làm OrderCode để tránh trùng với test data cũ trên PayOS sandbox
        var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        payment.TransactionRef = orderCode.ToString();
        await paymentRepository.SaveChangesAsync();

        var request = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)booking.DepositPaid,
            // PayOS yêu cầu description ASCII-only, tối đa 25 ký tự, dùng làm nội dung CK ngân hàng
            Description = $"BOOKING #{booking.Id}",
            ReturnUrl = _settings.ReturnUrl,
            CancelUrl = _settings.CancelUrl,
            Items =
            [
                new PaymentLinkItem
                {
                    Name     = "Dat coc bai do xe",
                    Quantity = 1,
                    Price    = (int)booking.DepositPaid
                }
            ]
        };

        var result = await payOsClient.PaymentRequests.CreateAsync(request);
        return result.CheckoutUrl;
    }

    public async Task HandleWebhookAsync(string rawBody)
    {
        // 1. Deserialize JSON body từ PayOS thành Webhook object
        var webhook = JsonSerializer.Deserialize<Webhook>(rawBody, _jsonOptions)
            ?? throw new InvalidOperationException("Webhook body không hợp lệ.");

        // 2. Xác thực chữ ký HMAC-SHA256 — throws nếu signature sai
        var webhookData = await payOsClient.Webhooks.VerifyAsync(webhook);

        // orderCode = Unix timestamp ms được lưu trong TransactionRef khi tạo link
        var payment = await paymentRepository.GetByTransactionRefAsync(webhookData.OrderCode.ToString());
        if (payment?.BookingId == null) return;

        // Idempotency: bỏ qua nếu webhook được gửi lại nhiều lần
        if (payment.Status != "pending") return;

        var booking = await bookingRepository.GetByIdAsync(payment.BookingId.Value);
        if (booking == null) return;

        if (webhookData.Code == "00")
        {
            // Thanh toán thành công
            payment.Status = "completed";
            payment.PaidAt = VnTime.Now;
            payment.TransactionRef = webhookData.Reference; // mã tham chiếu giao dịch ngân hàng

            // Booking confirmed → slot bị giữ (VFloorAvailability view tự tính confirmed bookings)
            booking.Status = "confirmed";
            booking.QrCode = Guid.NewGuid().ToString("N").ToUpper();
            booking.UpdatedAt = VnTime.Now;
        }
        else
        {
            // Thanh toán thất bại hoặc bị hủy
            payment.Status = "cancelled";

            booking.Status = "cancelled";
            booking.CancelReason = "Thanh toán không thành công hoặc bị hủy.";
            booking.UpdatedAt = VnTime.Now;
        }

        // Cả Payment lẫn Booking đều tracked trong cùng DbContext → 1 lần SaveChanges
        await paymentRepository.SaveChangesAsync();
    }

    public async Task<string> RetryPaymentLinkAsync(int bookingId, int userId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new AppException("Booking không tồn tại.", StatusCodes.Status404NotFound);

        if (booking.UserId != userId)
            throw new AppException("Bạn không có quyền truy cập booking này.",
                StatusCodes.Status403Forbidden);

        if (booking.Status != "pending_payment")
            throw new AppException(
                $"Booking đang ở trạng thái '{booking.Status}', không thể tạo link thanh toán mới.",
                StatusCodes.Status400BadRequest);

        // Hủy payment pending cũ để tránh nhiều link active cùng lúc
        var oldPayment = await paymentRepository.GetPendingByBookingIdAsync(bookingId);
        if (oldPayment != null)
            oldPayment.Status = "cancelled";

        var payment = new Payment
        {
            BookingId     = bookingId,
            Amount        = booking.DepositPaid,
            PaymentType   = "deposit",
            PaymentMethod = "payos",
            Status        = "pending",
            CreatedAt     = VnTime.Now
        };
        await paymentRepository.AddAsync(payment);
        await paymentRepository.SaveChangesAsync(); // lấy payment.Id mới làm orderCode

        return await CreatePaymentLinkAsync(booking, payment);
    }
}
