using System.ComponentModel.DataAnnotations;

namespace ParkingBuildingManagementSystem_BE.DTOs;

public class VehicleTypeRequest
{
    [Required(ErrorMessage = "Tên loại phương tiện là bắt buộc")]
    [MaxLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
    public string Name { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
    public string? Description { get; set; }
}
