namespace ParkingBuildingManagementSystem_BE.Services.Interfaces;

public interface IQrCodeService
{
    /// <summary>
    /// Tạo QR code PNG từ nội dung, trả về base64 string để nhúng trực tiếp vào JSON.
    /// </summary>
    string GenerateBase64(string content);
}
