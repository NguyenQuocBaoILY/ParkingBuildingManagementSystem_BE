using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<string> VerifyEmailAsync(VerifyEmailRequest request);
}
