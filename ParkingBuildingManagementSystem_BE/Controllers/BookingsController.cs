using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách booking của user đang đăng nhập.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "DriverOnly")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyBookings()
    {
        try
        {
            var userIdClaim = User.FindFirstValue("sub")
                ?? throw new AppException("Không xác định được người dùng.", StatusCodes.Status401Unauthorized);

            var userId = int.Parse(userIdClaim);
            var result = await bookingService.GetMyBookingsAsync(userId);
            return Ok(result);
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết 1 booking theo ID. Chỉ xem được booking của chính mình.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "DriverOnly")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue("sub")
                ?? throw new AppException("Không xác định được người dùng.", StatusCodes.Status401Unauthorized);

            var userId = int.Parse(userIdClaim);
            var result = await bookingService.GetByIdAsync(userId, id);
            return Ok(result);
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Hủy booking. Chỉ cho phép khi status = pending_payment hoặc confirmed.
    /// Nếu confirmed + quá CheckinDeadline → tự động chuyển expired thay vì cancel, cọc mất luôn.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = "DriverOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue("sub")
                ?? throw new AppException("Không xác định được người dùng.", StatusCodes.Status401Unauthorized);

            await bookingService.CancelAsync(int.Parse(userIdClaim), id);
            return NoContent();
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Tạo booking mới. Chỉ Driver đã xác thực.
    /// Trạng thái khởi tạo: pending_payment — chưa chiếm slot cho đến khi thanh toán cọc.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "DriverOnly")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] BookingRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue("sub")
                ?? throw new AppException("Không xác định được người dùng.", StatusCodes.Status401Unauthorized);

            var userId = int.Parse(userIdClaim);
            var result = await bookingService.CreateAsync(userId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
