namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string toName, string token);
}
