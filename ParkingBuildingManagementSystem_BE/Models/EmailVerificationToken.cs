using System;

namespace ParkingBuildingManagementSystem_BE.Models;

public class EmailVerificationToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    // Giá trị token ngẫu nhiên (GUID/hex) dùng để tra cứu nhanh
    public string Token { get; set; } = null!;

    // "email_verification" | "password_reset"
    public string TokenType { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
