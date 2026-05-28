using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Implementations;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Tests;

/// <summary>
/// Unit tests cho toàn bộ luồng Booking:
///   Login → Chọn loại xe → Kiểm tra slot → Đặt chỗ → Thanh toán → QR code
///
/// NOTE: PaymentService.HandleWebhookAsync phụ thuộc PayOSClient (không có interface)
/// nên luồng "webhook xác nhận payment" được kiểm tra qua trạng thái booking đã confirmed
/// trong GetByIdAsync tests — phần PayOS cần integration test riêng.
/// </summary>
public class BookingServiceTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────────

    private readonly Mock<IVehicleTypeRepository> _vehicleTypeRepo = new();
    private readonly Mock<IFloorRepository> _floorRepo = new();
    private readonly Mock<IVehicleRepository> _vehicleRepo = new();
    private readonly Mock<IPricingPolicyRepository> _pricingPolicyRepo = new();
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IPaymentService> _paymentService = new();
    private readonly Mock<IQrCodeService> _qrCodeService = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _sut = new BookingService(
            _vehicleTypeRepo.Object,
            _floorRepo.Object,
            _vehicleRepo.Object,
            _pricingPolicyRepo.Object,
            _bookingRepo.Object,
            _paymentRepo.Object,
            _paymentService.Object,
            _qrCodeService.Object,
            NullLogger<BookingService>.Instance);
    }

    // ── Shared test data ──────────────────────────────────────────────────────

    private static readonly DateTime FutureCheckin = VnTime.Now.AddDays(1);
    private static readonly DateTime PastCheckin   = VnTime.Now.AddDays(-1);

    private static VehicleType BuildVehicleType(int id = 1, string name = "Xe máy") =>
        new() { Id = id, Name = name };

    private static FloorSlotDto BuildFloorSlot(int floorId = 1, int available = 5) => new()
    {
        FloorId        = floorId,
        FloorName      = $"Tầng {floorId}",
        FloorNumber    = floorId,
        TotalSlots     = 20,
        OccupiedSlots  = 5,
        ReservedSlots  = 10,
        AvailableSlots = available
    };

    private static PricingPolicy BuildPolicy(decimal deposit = 50000, decimal pricePerHour = 10000) => new()
    {
        Id            = 1,
        VehicleTypeId = 1,
        FloorId       = 1,
        DepositAmount = deposit,
        PricePerHour  = pricePerHour,
        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1))
    };

    private static Vehicle BuildVehicle(int id = 10, int vehicleTypeId = 1, string plate = "51A-12345") => new()
    {
        Id            = id,
        UserId        = 1,
        VehicleTypeId = vehicleTypeId,
        LicensePlate  = plate
    };

    private static BookingRequest BuildRequest(
        int vehicleTypeId    = 1,
        int floorId          = 1,
        string licensePlate  = "51A-12345",
        DateTime? checkin    = null) => new()
    {
        VehicleTypeId   = vehicleTypeId,
        FloorId         = floorId,
        LicensePlate    = licensePlate,
        ScheduledCheckin = checkin ?? FutureCheckin
    };

    /// <summary>
    /// Setup happy-path mocks cho CreateAsync: vehicleType, floor có slot, policy, không có active booking.
    /// </summary>
    private void SetupHappyPathMocks(Vehicle? existingVehicle = null)
    {
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot(floorId: 1, available: 5) });
        _pricingPolicyRepo.Setup(r => r.GetActiveByVehicleTypeAndFloorAsync(1, 1))
                          .ReturnsAsync(BuildPolicy());
        _vehicleRepo.Setup(r => r.GetByLicensePlateAsync(It.IsAny<string>()))
                    .ReturnsAsync(existingVehicle);
        _bookingRepo.Setup(r => r.ExistsActiveByVehicleIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(false);
        _bookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        _bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
        _paymentRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _paymentService.Setup(p => p.CreatePaymentLinkAsync(It.IsAny<Booking>(), It.IsAny<Payment>()))
                       .ReturnsAsync("https://pay.payos.vn/checkout/test");
    }

    // =========================================================================
    // CreateAsync — happy cases
    // =========================================================================

    [Fact]
    public async Task CreateAsync_NewVehicle_CreatesVehicleAndReturnsBookingWithPaymentUrl()
    {
        // Arrange — xe chưa có trong hệ thống → service tự tạo
        SetupHappyPathMocks(existingVehicle: null);
        _vehicleRepo.Setup(r => r.AddAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);
        _vehicleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = BuildRequest();

        // Act
        var result = await _sut.CreateAsync(userId: 1, request);

        // Assert
        result.Status.Should().Be("pending_payment");
        result.VehicleTypeName.Should().Be("Xe máy");
        result.LicensePlate.Should().Be("51A-12345");
        result.DepositAmount.Should().Be(50000);
        result.PricePerHour.Should().Be(10000);
        result.PaymentUrl.Should().Be("https://pay.payos.vn/checkout/test");
        result.QrCodeImage.Should().BeNull(); // QR chỉ có sau khi confirmed

        // Vehicle mới phải được persist
        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ExistingVehicleSameType_ReusesVehicleWithoutCreatingNew()
    {
        // Arrange — xe đã tồn tại với đúng loại
        var existing = BuildVehicle(vehicleTypeId: 1);
        SetupHappyPathMocks(existingVehicle: existing);

        var request = BuildRequest();

        // Act
        var result = await _sut.CreateAsync(userId: 1, request);

        // Assert — xe cũ được dùng lại, không tạo mới
        result.Status.Should().Be("pending_payment");
        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_LicensePlateNormalized_UppercaseAndTrimmed()
    {
        // Arrange
        SetupHappyPathMocks(existingVehicle: null);
        _vehicleRepo.Setup(r => r.AddAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);
        _vehicleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = BuildRequest(licensePlate: "  51a-12345  ");

        // Act
        var result = await _sut.CreateAsync(userId: 1, request);

        // Assert — biển số phải được lookup sau khi normalize
        _vehicleRepo.Verify(r => r.GetByLicensePlateAsync("51A-12345"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PayOsThrows_StillReturnsBookingWithNullPaymentUrl()
    {
        // Arrange — PayOS bị lỗi nhưng booking vẫn phải được tạo
        SetupHappyPathMocks(existingVehicle: null);
        _vehicleRepo.Setup(r => r.AddAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);
        _vehicleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _paymentService.Setup(p => p.CreatePaymentLinkAsync(It.IsAny<Booking>(), It.IsAny<Payment>()))
                       .ThrowsAsync(new Exception("PayOS timeout"));

        var request = BuildRequest();

        // Act
        var result = await _sut.CreateAsync(userId: 1, request);

        // Assert — booking tồn tại, FE sẽ dùng retry endpoint
        result.Status.Should().Be("pending_payment");
        result.PaymentUrl.Should().BeNull();
    }

    // =========================================================================
    // CreateAsync — unhappy cases
    // =========================================================================

    [Fact]
    public async Task CreateAsync_PastScheduledCheckin_Throws400()
    {
        // Arrange
        var request = BuildRequest(checkin: PastCheckin);

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_VehicleTypeNotFound_Throws404()
    {
        // Arrange
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VehicleType?)null);

        var request = BuildRequest(vehicleTypeId: 99);

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateAsync_FloorNotSupportingVehicleType_Throws400()
    {
        // Arrange — floor trả về chỉ có FloorId = 2, request gửi FloorId = 1
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot(floorId: 2) });

        var request = BuildRequest(floorId: 1); // floor 1 không có trong list

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_NoAvailableSlots_Throws409()
    {
        // Arrange — floor hỗ trợ loại xe nhưng available = 0
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot(floorId: 1, available: 0) });

        var request = BuildRequest();

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task CreateAsync_VehicleTypeMismatch_Throws409()
    {
        // Arrange — biển số đã đăng ký là loại xe khác (typeId=2 nhưng request gửi typeId=1)
        var mismatchVehicle = BuildVehicle(vehicleTypeId: 2);

        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot() });
        _vehicleRepo.Setup(r => r.GetByLicensePlateAsync(It.IsAny<string>()))
                    .ReturnsAsync(mismatchVehicle);

        var request = BuildRequest(vehicleTypeId: 1);

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task CreateAsync_VehicleAlreadyHasActiveBooking_Throws409()
    {
        // Arrange — xe đang có booking pending_payment hoặc confirmed
        var existing = BuildVehicle(vehicleTypeId: 1);

        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot() });
        _vehicleRepo.Setup(r => r.GetByLicensePlateAsync(It.IsAny<string>())).ReturnsAsync(existing);
        _bookingRepo.Setup(r => r.ExistsActiveByVehicleIdAsync(existing.Id)).ReturnsAsync(true);

        var request = BuildRequest();

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task CreateAsync_NoPricingPolicy_Throws404()
    {
        // Arrange
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildVehicleType());
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1))
                  .ReturnsAsync(new[] { BuildFloorSlot() });
        _vehicleRepo.Setup(r => r.GetByLicensePlateAsync(It.IsAny<string>()))
                    .ReturnsAsync((Vehicle?)null);
        _vehicleRepo.Setup(r => r.AddAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);
        _vehicleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _bookingRepo.Setup(r => r.ExistsActiveByVehicleIdAsync(It.IsAny<int>())).ReturnsAsync(false);
        _pricingPolicyRepo.Setup(r => r.GetActiveByVehicleTypeAndFloorAsync(1, 1))
                          .ReturnsAsync((PricingPolicy?)null);

        var request = BuildRequest();

        // Act
        var act = () => _sut.CreateAsync(userId: 1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    // =========================================================================
    // GetByIdAsync — QR code sau khi payment confirmed
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_ConfirmedBookingWithQrCode_ReturnsQrCodeImage()
    {
        // Arrange — giả lập trạng thái sau khi PayOS webhook đã confirmed booking
        var confirmedBooking = new Booking
        {
            Id               = 1,
            UserId           = 1,
            VehicleTypeId    = 1,
            FloorId          = 1,
            Status           = "confirmed",
            QrCode           = "A1B2C3D4E5F6",   // đã được set bởi webhook handler
            ScheduledCheckin = FutureCheckin,
            CheckinDeadline  = FutureCheckin.AddMinutes(30),
            DepositPaid      = 50000,
            CreatedAt        = VnTime.Now,
            UpdatedAt        = VnTime.Now,
            VehicleType      = BuildVehicleType(),
            Vehicle          = BuildVehicle(),
            Floor            = new Floor { Id = 1, Name = "Tầng 1", FloorNumber = 1 }
        };

        _bookingRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(confirmedBooking);
        _pricingPolicyRepo.Setup(r => r.GetActiveByVehicleTypeAndFloorAsync(1, 1))
                          .ReturnsAsync(BuildPolicy());
        _qrCodeService.Setup(q => q.GenerateBase64("A1B2C3D4E5F6"))
                      .Returns("iVBORw0KGgoAAAANSUhEUgAA...base64png");

        // Act
        var result = await _sut.GetByIdAsync(userId: 1, bookingId: 1);

        // Assert — QR image phải có mặt khi booking confirmed
        result.Status.Should().Be("confirmed");
        result.QrCodeImage.Should().NotBeNullOrEmpty();
        result.QrCodeImage.Should().Be("iVBORw0KGgoAAAANSUhEUgAA...base64png");

        _qrCodeService.Verify(q => q.GenerateBase64("A1B2C3D4E5F6"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_PendingPaymentBooking_QrCodeImageIsNull()
    {
        // Arrange — booking chưa thanh toán, QrCode chưa được set
        var pendingBooking = new Booking
        {
            Id               = 2,
            UserId           = 1,
            VehicleTypeId    = 1,
            FloorId          = 1,
            Status           = "pending_payment",
            QrCode           = null,
            ScheduledCheckin = FutureCheckin,
            CheckinDeadline  = FutureCheckin.AddMinutes(30),
            DepositPaid      = 50000,
            CreatedAt        = VnTime.Now,
            UpdatedAt        = VnTime.Now,
            VehicleType      = BuildVehicleType(),
            Vehicle          = BuildVehicle(),
            Floor            = new Floor { Id = 1, Name = "Tầng 1", FloorNumber = 1 }
        };

        _bookingRepo.Setup(r => r.GetByIdWithDetailsAsync(2)).ReturnsAsync(pendingBooking);
        _pricingPolicyRepo.Setup(r => r.GetActiveByVehicleTypeAndFloorAsync(1, 1))
                          .ReturnsAsync(BuildPolicy());

        // Act
        var result = await _sut.GetByIdAsync(userId: 1, bookingId: 2);

        // Assert — chưa có QR vì chưa thanh toán
        result.Status.Should().Be("pending_payment");
        result.QrCodeImage.Should().BeNull();

        _qrCodeService.Verify(q => q.GenerateBase64(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_BookingNotFound_Throws404()
    {
        // Arrange
        _bookingRepo.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((Booking?)null);

        // Act
        var act = () => _sut.GetByIdAsync(userId: 1, bookingId: 99);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByIdAsync_BookingBelongsToDifferentUser_Throws403()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1, UserId = 99, // thuộc user khác
            VehicleTypeId = 1, FloorId = 1, Status = "confirmed",
            ScheduledCheckin = FutureCheckin, CheckinDeadline = FutureCheckin.AddMinutes(30),
            DepositPaid = 50000, CreatedAt = VnTime.Now, UpdatedAt = VnTime.Now,
            VehicleType = BuildVehicleType(), Vehicle = BuildVehicle(),
            Floor = new Floor { Id = 1, Name = "Tầng 1", FloorNumber = 1 }
        };

        _bookingRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act — user 1 cố xem booking của user 99
        var act = () => _sut.GetByIdAsync(userId: 1, bookingId: 1);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(403);
    }

    // =========================================================================
    // CancelAsync — happy cases
    // =========================================================================

    [Fact]
    public async Task CancelAsync_PendingPaymentBooking_CancelsSuccessfully()
    {
        // Arrange — booking chưa thanh toán, cancel thoải mái
        var booking = new Booking
        {
            Id     = 1,
            UserId = 1,
            Status = "pending_payment",
            ScheduledCheckin = FutureCheckin,
            CheckinDeadline  = FutureCheckin.AddMinutes(30)
        };

        _bookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.CancelAsync(userId: 1, bookingId: 1);

        // Assert
        booking.Status.Should().Be("cancelled");
        booking.CancelReason.Should().NotBeNullOrEmpty();
        _bookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedBookingWithinDeadline_CancelsAndFreesSlot()
    {
        // Arrange — booking đã confirmed nhưng còn trong hạn checkin
        var booking = new Booking
        {
            Id     = 1,
            UserId = 1,
            Status = "confirmed",
            ScheduledCheckin = FutureCheckin,
            CheckinDeadline  = FutureCheckin.AddMinutes(30) // còn hạn
        };

        _bookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.CancelAsync(userId: 1, bookingId: 1);

        // Assert — slot được giải phóng tự động qua VFloorAvailability view
        booking.Status.Should().Be("cancelled");
        booking.CancelReason.Should().NotBeNullOrEmpty();
    }

    // =========================================================================
    // CancelAsync — unhappy cases
    // =========================================================================

    [Fact]
    public async Task CancelAsync_BookingNotFound_Throws404()
    {
        // Arrange
        _bookingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Booking?)null);

        // Act
        var act = () => _sut.CancelAsync(userId: 1, bookingId: 99);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CancelAsync_NotBookingOwner_Throws403()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1, UserId = 99, Status = "confirmed",
            ScheduledCheckin = FutureCheckin, CheckinDeadline = FutureCheckin.AddMinutes(30)
        };
        _bookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act — user 1 cố cancel booking của user 99
        var act = () => _sut.CancelAsync(userId: 1, bookingId: 1);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(403);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("cancelled")]
    [InlineData("completed")]
    public async Task CancelAsync_IrreversibleStatus_Throws400(string status)
    {
        // Arrange — booking ở trạng thái không thể hủy
        var booking = new Booking
        {
            Id = 1, UserId = 1, Status = status,
            ScheduledCheckin = FutureCheckin, CheckinDeadline = FutureCheckin.AddMinutes(30)
        };
        _bookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act
        var act = () => _sut.CancelAsync(userId: 1, bookingId: 1);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedButPastCheckinDeadline_AutoExpiresAndThrows409()
    {
        // Arrange — booking confirmed nhưng đã quá CheckinDeadline
        // → không được cancel, hệ thống tự chuyển sang expired, cọc mất
        var booking = new Booking
        {
            Id     = 1,
            UserId = 1,
            Status = "confirmed",
            ScheduledCheckin = VnTime.Now.AddDays(-2),
            CheckinDeadline  = VnTime.Now.AddHours(-1) // đã quá hạn
        };

        _bookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var act = () => _sut.CancelAsync(userId: 1, bookingId: 1);

        // Assert — trả về 409, booking bị expire tại đây
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(409);
        ex.Which.Message.Should().Contain("expired");

        booking.Status.Should().Be("expired"); // auto-expire side effect
        _bookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
