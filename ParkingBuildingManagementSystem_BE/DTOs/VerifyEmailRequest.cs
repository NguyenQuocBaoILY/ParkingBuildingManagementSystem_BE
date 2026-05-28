using System.ComponentModel.DataAnnotations;

namespace ParkingBuildingManagementSystem_BE.DTOs;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mã OTP không được để trống.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ gồm 6 chữ số.")]
    public string Otp { get; set; } = null!;
}
