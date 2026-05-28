using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    int ExpiresInSeconds { get; }
}
