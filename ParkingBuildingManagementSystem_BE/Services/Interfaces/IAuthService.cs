using ParkingBuildingManagementSystem_BE.DTOs;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
}
