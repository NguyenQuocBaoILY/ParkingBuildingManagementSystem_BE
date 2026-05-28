using System.ComponentModel.DataAnnotations;

namespace ParkingBuildingManagementSystem_BE.DTOs;

public class BookingRequest
{
    [Required(ErrorMessage = "Loại phương tiện là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId không hợp lệ")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Tầng đỗ xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "FloorId không hợp lệ")]
    public int FloorId { get; set; }

    [Required(ErrorMessage = "Biển số xe là bắt buộc")]
    [MaxLength(20, ErrorMessage = "Biển số không được vượt quá 20 ký tự")]
    public string LicensePlate { get; set; } = null!;

    [Required(ErrorMessage = "Thời gian dự kiến đỗ xe là bắt buộc")]
    public DateTime ScheduledCheckin { get; set; }
}
