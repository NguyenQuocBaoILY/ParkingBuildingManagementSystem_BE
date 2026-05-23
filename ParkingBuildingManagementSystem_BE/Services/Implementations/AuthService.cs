using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email)
            ?? throw new Exception("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return "token_placeholder";
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer"
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();
    }
}
