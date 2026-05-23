using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
