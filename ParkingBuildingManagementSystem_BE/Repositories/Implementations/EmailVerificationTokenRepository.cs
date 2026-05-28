using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Data;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Repositories.Implementations;

public class EmailVerificationTokenRepository(AppDbContext context) : IEmailVerificationTokenRepository
{
    public async Task AddAsync(EmailVerificationToken token) =>
        await context.EmailVerificationTokens.AddAsync(token);

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token) =>
        await context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

    public async Task<EmailVerificationToken?> GetByEmailAndOtpAsync(string email, string otp) =>
        await context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.User.Email == email && t.Token == otp);

    public async Task SaveChangesAsync() =>
        await context.SaveChangesAsync();
}
