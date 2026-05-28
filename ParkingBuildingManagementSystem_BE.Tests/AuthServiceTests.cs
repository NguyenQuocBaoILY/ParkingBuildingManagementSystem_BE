using FluentAssertions;
using Moq;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Implementations;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmailVerificationTokenRepository> _tokenRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly AuthService _sut;

    // Dùng password thật để BCrypt.Verify hoạt động đúng
    private const string ValidPassword = "ValidPass123!";
    private static readonly string ValidPasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword);

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepo.Object, _tokenRepo.Object, _emailService.Object, _jwtService.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private User BuildActiveVerifiedUser(string email = "driver@test.com") => new()
    {
        Id              = 1,
        Email           = email,
        PasswordHash    = ValidPasswordHash,
        FullName        = "Test Driver",
        Role            = "driver",
        IsActive        = true,
        IsEmailVerified = true
    };

    // ── LoginAsync — happy path ───────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = BuildActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("driver@test.com")).ReturnsAsync(user);
        _jwtService.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwtService.Setup(j => j.ExpiresInSeconds).Returns(3600);

        var request = new LoginRequest { Email = "driver@test.com", Password = ValidPassword };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.AccessToken.Should().Be("jwt-token");
        result.TokenType.Should().Be("Bearer");
        result.Email.Should().Be("driver@test.com");
        result.Role.Should().Be("driver");
    }

    [Fact]
    public async Task LoginAsync_EmailIsCaseInsensitive_NormalizesBeforeLookup()
    {
        // Arrange
        var user = BuildActiveVerifiedUser("driver@test.com");
        _userRepo.Setup(r => r.GetByEmailAsync("driver@test.com")).ReturnsAsync(user);
        _jwtService.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");
        _jwtService.Setup(j => j.ExpiresInSeconds).Returns(3600);

        var request = new LoginRequest { Email = "  DRIVER@TEST.COM  ", Password = ValidPassword };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert — service phải trim + lowercase trước khi lookup
        result.Should().NotBeNull();
        _userRepo.Verify(r => r.GetByEmailAsync("driver@test.com"), Times.Once);
    }

    // ── LoginAsync — unhappy paths ────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_UserNotFound_Throws401()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nobody@test.com", Password = ValidPassword };

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_Throws401()
    {
        // Arrange
        var user = BuildActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var request = new LoginRequest { Email = "driver@test.com", Password = "WrongPassword!" };

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert — sai mật khẩu phải cùng status code với user not found để tránh user enumeration
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_Throws403()
    {
        // Arrange
        var user = BuildActiveVerifiedUser();
        user.IsActive = false;
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var request = new LoginRequest { Email = "driver@test.com", Password = ValidPassword };

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task LoginAsync_EmailNotVerified_Throws403()
    {
        // Arrange
        var user = BuildActiveVerifiedUser();
        user.IsEmailVerified = false;
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var request = new LoginRequest { Email = "driver@test.com", Password = ValidPassword };

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(403);
        ex.Which.Message.Should().Contain("xác minh");
    }
}
