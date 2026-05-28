using System.Security.Cryptography;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class AuthService(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository tokenRepository,
    IEmailService emailService,
    IJwtService jwtService) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.ToLower().Trim();

        var user = await userRepository.GetByEmailAsync(normalizedEmail)
            ?? throw new AppException("Thông tin đăng nhập không hợp lệ.", 401);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Thông tin đăng nhập không hợp lệ.", 401);

        if (!user.IsActive)
            throw new AppException("Tài khoản đã bị vô hiệu hóa.", 403);

        if (!user.IsEmailVerified)
            throw new AppException("Tài khoản chưa được xác minh email. Vui lòng kiểm tra hộp thư và xác minh trước khi đăng nhập.", 403);

        return new AuthResponse
        {
            AccessToken = jwtService.GenerateToken(user),
            TokenType   = "Bearer",
            ExpiresIn   = jwtService.ExpiresInSeconds,
            Email       = user.Email,
            FullName    = user.FullName,
            Role        = user.Role
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.ToLower().Trim();

        var existing = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existing is not null)
            throw new AppException("Email này đã được đăng ký.", 409);

        var now = VnTime.Now;

        var user = new User
        {
            FullName        = request.FullName.Trim(),
            Email           = normalizedEmail,
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role            = "driver",
            IsActive        = true,
            IsEmailVerified = false,
            CreatedAt       = now,
            UpdatedAt       = now
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        // OTP 6 chữ số, dùng RandomNumberGenerator để đảm bảo an toàn mật mã
        var tokenValue = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var verificationToken = new EmailVerificationToken
        {
            UserId    = user.Id,
            Token     = tokenValue,
            TokenType = "email_verification",
            ExpiresAt = now.AddMinutes(15),
            IsUsed    = false,
            CreatedAt = now
        };

        await tokenRepository.AddAsync(verificationToken);
        await tokenRepository.SaveChangesAsync();

        await emailService.SendEmailVerificationAsync(user.Email, user.FullName, tokenValue);

        return new RegisterResponse
        {
            Message                   = "Đăng ký thành công. Vui lòng kiểm tra email để xác minh tài khoản.",
            Email                     = user.Email,
            RequiresEmailVerification = true
        };
    }

    public async Task<string> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var normalizedEmail = request.Email.ToLower().Trim();

        var verificationToken = await tokenRepository.GetByEmailAndOtpAsync(normalizedEmail, request.Otp)
            ?? throw new AppException("Mã OTP không hợp lệ.", 400);

        if (verificationToken.TokenType != "email_verification")
            throw new AppException("Mã OTP không hợp lệ.", 400);

        if (verificationToken.IsUsed)
            throw new AppException("Mã OTP đã được sử dụng.", 400);

        if (verificationToken.ExpiresAt < VnTime.Now)
            throw new AppException("Mã OTP đã hết hạn. Vui lòng đăng ký lại để nhận mã mới.", 400);

        verificationToken.IsUsed = true;
        verificationToken.User.IsEmailVerified = true;

        await tokenRepository.SaveChangesAsync();

        return "Xác minh email thành công. Tài khoản của bạn đã được kích hoạt.";
    }
}
