namespace ParkingBuildingManagementSystem_BE.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;

    // Vehicle info
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public string LicensePlate { get; set; } = null!;

    // Floor info
    public int FloorId { get; set; }
    public string FloorName { get; set; } = null!;
    public int FloorNumber { get; set; }

    // Timing
    public DateTime ScheduledCheckin { get; set; }
    public DateTime CheckinDeadline { get; set; }

    // Payment
    public decimal DepositAmount { get; set; }
    public decimal PricePerHour { get; set; }
    public string? PaymentUrl { get; set; }

    // QR code PNG dạng base64 — chỉ có khi status = "confirmed"
    public string? QrCodeImage { get; set; }

    public DateTime CreatedAt { get; set; }
}
