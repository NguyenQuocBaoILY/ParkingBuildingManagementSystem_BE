using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token);
    Task<EmailVerificationToken?> GetByTokenAsync(string token);
    Task<EmailVerificationToken?> GetByEmailAndOtpAsync(string email, string otp);
    Task SaveChangesAsync();
}
