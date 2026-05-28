using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class BookingService(
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
    // Khoảng thời gian gia hạn: người dùng phải check-in trong vòng X phút sau ScheduledCheckin
    private const int CheckinGraceMinutes = 30;

    public async Task<BookingResponse> CreateAsync(int userId, BookingRequest request)
    {
        // 1. Chuẩn hoá biển số
        var licensePlate = request.LicensePlate.Trim().ToUpper();

        // 2. Validate thời gian đặt chỗ phải trong tương lai
        if (request.ScheduledCheckin <= VnTime.Now)
            throw new AppException("Thời gian đỗ xe phải là thời điểm trong tương lai.",
                StatusCodes.Status400BadRequest);

        // 3. Kiểm tra loại phương tiện tồn tại
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        // 4. Kiểm tra tầng hỗ trợ loại xe này và còn slot trống
        var supportedFloors = (await floorRepository.GetAvailableByVehicleTypeAsync(request.VehicleTypeId)).ToList();
        var targetFloor = supportedFloors.FirstOrDefault(f => f.FloorId == request.FloorId)
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
            // Biển số chưa có trong hệ thống → tự tạo, gắn với user hiện tại làm người đăng ký
            vehicle = new Vehicle
            {
                UserId        = userId,
                VehicleTypeId = request.VehicleTypeId,
                LicensePlate  = licensePlate
            };
            await vehicleRepository.AddAsync(vehicle);
            await vehicleRepository.SaveChangesAsync(); // cần Id trước khi tạo booking
        }
        else if (vehicle.VehicleTypeId != request.VehicleTypeId)
        {
            // Biển số đã được đăng ký nhưng là loại xe khác — đây là dữ liệu không nhất quán thật sự
            throw new AppException(
                "Biển số này đã được đăng ký là loại phương tiện khác. Vui lòng chọn đúng loại.",
                StatusCodes.Status409Conflict);
        }

        // 6. 1 xe chỉ được có 1 booking active tại 1 thời điểm — xe vật lý không thể đỗ 2 nơi
        if (await bookingRepository.ExistsActiveByVehicleIdAsync(vehicle.Id))
            throw new AppException(
                "Xe này đang có booking chưa hoàn tất. Không thể đặt thêm.",
                StatusCodes.Status409Conflict);

        // 7. Lấy chính sách giá đang hiệu lực (floor-specific ưu tiên hơn global)
        var policy = await pricingPolicyRepository.GetActiveByVehicleTypeAndFloorAsync(
            request.VehicleTypeId, request.FloorId)
            ?? throw new AppException("Chưa có chính sách giá cho tầng và loại xe này.",
                StatusCodes.Status404NotFound);

        // 8. Tạo booking
        var checkinDeadline = request.ScheduledCheckin.AddMinutes(CheckinGraceMinutes);

        var booking = new Booking
        {
            UserId           = userId,
            VehicleId        = vehicle.Id,
            VehicleTypeId    = request.VehicleTypeId,
            FloorId          = request.FloorId,
            ScheduledCheckin = request.ScheduledCheckin,
            CheckinDeadline  = checkinDeadline,
            DepositPaid      = policy.DepositAmount,
            Status           = "pending_payment",
            CreatedAt        = VnTime.Now,
            UpdatedAt        = VnTime.Now
        };

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // 9. Tạo Payment record (status: pending) — dùng payment.Id làm orderCode gửi lên PayOS
        var payment = new Payment
        {
            BookingId     = booking.Id,
            Amount        = policy.DepositAmount,
            PaymentType   = "deposit",
            PaymentMethod = "online",
            Status        = "pending",
            CreatedAt     = VnTime.Now
        };

        await paymentRepository.AddAsync(payment);
        await paymentRepository.SaveChangesAsync(); // cần payment.Id trước khi gọi PayOS

        // 10. Tạo PayOS checkout link — nếu fail thì vẫn trả về booking, FE dùng retry endpoint
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
            LicensePlate     = vehicle.LicensePlate,
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
