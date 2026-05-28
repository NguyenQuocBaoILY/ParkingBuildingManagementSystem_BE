using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;
using ParkingBuildingManagementSystem_BE.Settings;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class EmailService(IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendEmailVerificationAsync(string toEmail, string toName, string token)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Mã xác minh email của bạn";
        message.Body = new BodyBuilder
        {
            HtmlBody = BuildOtpEmailHtml(toName, token)
        }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions);
        await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string BuildOtpEmailHtml(string name, string otp) => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; margin: 0;">
          <div style="max-width: 600px; margin: 40px auto; background: #ffffff;
                      padding: 32px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
            <h2 style="color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 12px; margin-top: 0;">
              Xác minh địa chỉ Email
            </h2>
            <p style="color: #34495e;">Xin chào <strong>{name}</strong>,</p>
            <p style="color: #34495e;">
              Cảm ơn bạn đã đăng ký tài khoản tại <strong>Parking Building System</strong>.<br/>
              Sử dụng mã OTP bên dưới để xác minh địa chỉ email của bạn.
            </p>
            <div style="text-align: center; margin: 32px 0;">
              <div style="display: inline-block; background-color: #f0f8ff; border: 2px solid #3498db;
                          border-radius: 8px; padding: 16px 40px;">
                <span style="font-size: 36px; font-weight: bold; color: #2c3e50;
                             letter-spacing: 8px; font-family: monospace;">
                  {otp}
                </span>
              </div>
            </div>
            <p style="color: #95a5a6; font-size: 13px; border-top: 1px solid #ecf0f1; padding-top: 16px; margin-bottom: 0;">
              Mã OTP có hiệu lực trong <strong>15 phút</strong>.<br/>
              Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
            </p>
          </div>
        </body>
        </html>
        """;
}
