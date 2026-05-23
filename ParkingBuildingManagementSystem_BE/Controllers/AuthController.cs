using Microsoft.AspNetCore.Mvc;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await authService.LoginAsync(request);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await authService.RegisterAsync(request);
        return Ok(new { message = "Registered successfully" });
    }
}
