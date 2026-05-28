using System.ComponentModel.DataAnnotations;

namespace ParkingBuildingManagementSystem_BE.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [MaxLength(100, ErrorMessage = "Họ và tên không vượt quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(150, ErrorMessage = "Email không vượt quá 150 ký tự.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;
}
