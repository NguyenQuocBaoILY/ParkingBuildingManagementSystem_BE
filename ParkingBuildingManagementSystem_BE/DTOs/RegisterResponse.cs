namespace ParkingBuildingManagementSystem_BE.DTOs;

public class RegisterResponse
{
    public string Message { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool RequiresEmailVerification { get; set; }
}
