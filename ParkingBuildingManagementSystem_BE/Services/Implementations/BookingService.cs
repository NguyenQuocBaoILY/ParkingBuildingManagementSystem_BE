using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;
using ParkingBuildingManagementSystem_BE.Settings;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class BookingService(
    AppDbContext context,
    IOptions<BookingSettings> bookingOptions,
    IVehicleTypeRepository vehicleTypeRepository,
    IFloorRepository floorRepository,
    IVehicleRepository vehicleRepository,
    IPricingPolicyRepository pricingPolicyRepository,
    IBookingRepository bookingRepository,
    IPaymentRepository paymentRepository,
    IPaymentService paymentService,
    IQrCodeService qrCodeService,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly BookingSettings _bookingSettings = bookingOptions.Value;

    public async Task<BookingResponse> CreateAsync(int userId, BookingRequest request)
    {
        // 1. Chuẩn hoá biển số
        var licensePlate = request.LicensePlate.Trim().ToUpper();

        // 2. Validate thời gian đặt chỗ phải trong tương lai
        if (request.ScheduledCheckin <= VnTime.Now)
            throw new AppException("Thời gian đỗ xe phải là thời điểm trong tương lai.",
                StatusCodes.Status400BadRequest);

        // 3. Kiểm tra loại phương tiện tồn tại (ngoài transaction — read-only, không ảnh hưởng slot)
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        // Các bước kiểm tra slot, tạo vehicle/booking/payment chạy trong 1 transaction Serializable.
        // SQL Server sẽ giữ range lock trên rows được đọc → ngăn 2 request đồng thời cùng pass
        // kiểm tra AvailableSlots > 0 và cùng tạo booking cho cùng 1 tầng.
        Booking booking;
        Payment payment;
        FloorSlotDto targetFloor;
        PricingPolicy policy;

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // 4. Kiểm tra tầng hỗ trợ loại xe này và còn slot trống
            var supportedFloors = (await floorRepository.GetAvailableByVehicleTypeAsync(request.VehicleTypeId)).ToList();
            targetFloor = supportedFloors.FirstOrDefault(f => f.FloorId == request.FloorId)
                ?? throw new AppException("Tầng này không hỗ trợ loại phương tiện đã chọn.",
                    StatusCodes.Status400BadRequest);

            if (targetFloor.AvailableSlots <= 0)
                throw new AppException("Tầng này hiện không còn slot trống.", StatusCodes.Status409Conflict);

            // 5. Tìm xe theo biển số — không kiểm tra ownership vì người dùng có thể mượn xe người khác.
            //    Booking.UserId = người đặt chỗ (trả tiền, nhận QR).
            //    Vehicle.UserId = người đầu tiên đăng ký biển số vào hệ thống (chỉ dùng để tra cứu).
            var vehicle = await vehicleRepository.GetByLicensePlateAsync(licensePlate);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    UserId        = userId,
                    VehicleTypeId = request.VehicleTypeId,
                    LicensePlate  = licensePlate
                };
                await vehicleRepository.AddAsync(vehicle);
                await vehicleRepository.SaveChangesAsync();
            }
            else if (vehicle.VehicleTypeId != request.VehicleTypeId)
            {
                throw new AppException(
                    "Biển số này đã được đăng ký là loại phương tiện khác. Vui lòng chọn đúng loại.",
                    StatusCodes.Status409Conflict);
            }

            // 6. 1 xe chỉ được có 1 booking active tại 1 thời điểm
            if (await bookingRepository.ExistsActiveByVehicleIdAsync(vehicle.Id))
                throw new AppException(
                    "Xe này đang có booking chưa hoàn tất. Không thể đặt thêm.",
                    StatusCodes.Status409Conflict);

            // 7. Lấy chính sách giá đang hiệu lực (floor-specific ưu tiên hơn global)
            policy = await pricingPolicyRepository.GetActiveByVehicleTypeAndFloorAsync(
                request.VehicleTypeId, request.FloorId)
                ?? throw new AppException("Chưa có chính sách giá cho tầng và loại xe này.",
                    StatusCodes.Status404NotFound);

            // 8. Tạo booking
            booking = new Booking
            {
                UserId           = userId,
                VehicleId        = vehicle.Id,
                VehicleTypeId    = request.VehicleTypeId,
                FloorId          = request.FloorId,
                ScheduledCheckin = request.ScheduledCheckin,
                CheckinDeadline  = request.ScheduledCheckin.AddMinutes(_bookingSettings.CheckinGraceMinutes),
                DepositPaid      = policy.DepositAmount,
                Status           = "pending_payment",
                CreatedAt        = VnTime.Now,
                UpdatedAt        = VnTime.Now
            };

            await bookingRepository.AddAsync(booking);
            await bookingRepository.SaveChangesAsync();

            // 9. Tạo Payment record
            payment = new Payment
            {
                BookingId     = booking.Id,
                Amount        = policy.DepositAmount,
                PaymentType   = "deposit",
                PaymentMethod = "payos",
                Status        = "pending",
                CreatedAt     = VnTime.Now
            };

            await paymentRepository.AddAsync(payment);
            await paymentRepository.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        // 10. Tạo PayOS checkout link — ngoài transaction vì gọi external API không nên giữ DB lock
        string? paymentUrl = null;
        try
        {
            paymentUrl = await paymentService.CreatePaymentLinkAsync(booking, payment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể tạo PayOS link cho booking {BookingId}", booking.Id);
        }

        return new BookingResponse
        {
            Id               = booking.Id,
            Status           = booking.Status,
            VehicleTypeId    = vehicleType.Id,
            VehicleTypeName  = vehicleType.Name,
            LicensePlate     = licensePlate,
            FloorId          = targetFloor.FloorId,
            FloorName        = targetFloor.FloorName,
            FloorNumber      = targetFloor.FloorNumber,
            ScheduledCheckin = booking.ScheduledCheckin,
            CheckinDeadline  = booking.CheckinDeadline,
            DepositAmount    = policy.DepositAmount,
            PricePerHour     = policy.PricePerHour,
            PaymentUrl       = paymentUrl,
            CreatedAt        = booking.CreatedAt
        };
    }

    public async Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int userId)
    {
        var bookings = await bookingRepository.GetByUserIdAsync(userId);
        var responses = new List<BookingResponse>();

        foreach (var b in bookings)
        {
            var policy = await pricingPolicyRepository.GetActiveByVehicleTypeAndFloorAsync(b.VehicleTypeId, b.FloorId);
            responses.Add(MapToResponse(b, policy?.PricePerHour ?? 0));
        }

        return responses;
    }

    public async Task<BookingResponse> GetByIdAsync(int userId, int bookingId)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new AppException("Booking không tồn tại.", StatusCodes.Status404NotFound);

        if (booking.UserId != userId)
            throw new AppException("Bạn không có quyền xem booking này.", StatusCodes.Status403Forbidden);

        var policy = await pricingPolicyRepository.GetActiveByVehicleTypeAndFloorAsync(booking.VehicleTypeId, booking.FloorId);
        return MapToResponse(booking, policy?.PricePerHour ?? 0);
    }

    public async Task CancelAsync(int userId, int bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new AppException("Booking không tồn tại.", StatusCodes.Status404NotFound);

        if (booking.UserId != userId)
            throw new AppException("Bạn không có quyền hủy booking này.", StatusCodes.Status403Forbidden);

        if (booking.Status is not ("pending_payment" or "confirmed"))
            throw new AppException(
                $"Không thể hủy booking đang ở trạng thái '{booking.Status}'.",
                StatusCodes.Status400BadRequest);

        // Confirmed + quá CheckinDeadline → auto-expire, không cho cancel
        if (booking.Status == "confirmed" && VnTime.Now > booking.CheckinDeadline)
        {
            booking.Status    = "expired";
            booking.UpdatedAt = VnTime.Now;
            await bookingRepository.SaveChangesAsync();
            throw new AppException(
                "Booking đã quá hạn check-in và bị chuyển sang expired. Cọc không được hoàn.",
                StatusCodes.Status409Conflict);
        }

        booking.Status       = "cancelled";
        booking.CancelReason = "Người dùng tự hủy.";
        booking.UpdatedAt    = VnTime.Now;
        await bookingRepository.SaveChangesAsync();
    }

    private BookingResponse MapToResponse(Booking b, decimal pricePerHour) => new()
    {
        Id               = b.Id,
        Status           = b.Status,
        VehicleTypeId    = b.VehicleTypeId,
        VehicleTypeName  = b.VehicleType?.Name ?? string.Empty,
        LicensePlate     = b.Vehicle?.LicensePlate ?? string.Empty,
        FloorId          = b.FloorId,
        FloorName        = b.Floor?.Name ?? string.Empty,
        FloorNumber      = b.Floor?.FloorNumber ?? 0,
        ScheduledCheckin = b.ScheduledCheckin,
        CheckinDeadline  = b.CheckinDeadline,
        DepositAmount    = b.DepositPaid,
        PricePerHour     = pricePerHour,
        PaymentUrl       = null,
        QrCodeImage      = b.Status == "confirmed" && b.QrCode != null
                               ? qrCodeService.GenerateBase64(b.QrCode)
                               : null,
        CreatedAt        = b.CreatedAt
    };
}
