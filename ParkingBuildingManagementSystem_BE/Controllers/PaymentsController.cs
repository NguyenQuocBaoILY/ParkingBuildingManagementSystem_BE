using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ParkingBuildingManagementSystem_BE.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(
    IPaymentService paymentService,
    ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "PayOS webhook callback (không cần auth)")]
    public async Task<IActionResult> PayOsWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        try
        {
            await paymentService.HandleWebhookAsync(rawBody);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi xử lý PayOS webhook. Body: {Body}", rawBody);
            // Trả 200 để PayOS không retry — lỗi signature/data không cần retry
            return Ok(new { success = false });
        }
    }

    [HttpGet("success")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Landing page sau thanh toán thành công")]
    public IActionResult PaymentSuccess([FromQuery] string? code, [FromQuery] long? id, [FromQuery] string? cancel)
    {
        return Ok(new
        {
            message    = "Thanh toán thành công! Booking đã được xác nhận.",
            orderCode  = id,
            resultCode = code
        });
    }

    [HttpGet("cancel")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Landing page sau hủy thanh toán")]
    public IActionResult PaymentCancel([FromQuery] long? id)
    {
        return Ok(new
        {
            message   = "Đã hủy thanh toán. Booking bị hủy.",
            orderCode = id
        });
    }

    [HttpPost("booking/{bookingId:int}/link")]
    [Authorize(Policy = "DriverOnly")]
    [SwaggerOperation(Summary = "Tạo lại link thanh toán PayOS (DriverOnly)")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryPaymentLink(int bookingId)
    {
        var userIdStr = User.FindFirstValue("sub");
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Không xác định được người dùng." });

        try
        {
            var url = await paymentService.RetryPaymentLinkAsync(bookingId, userId);
            return Ok(new { paymentUrl = url });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
