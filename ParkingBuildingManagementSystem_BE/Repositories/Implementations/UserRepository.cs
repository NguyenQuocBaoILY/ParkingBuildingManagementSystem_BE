using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id) =>
        await context.Users.FindAsync(id);

    public async Task AddAsync(User user) =>
        await context.Users.AddAsync(user);

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
