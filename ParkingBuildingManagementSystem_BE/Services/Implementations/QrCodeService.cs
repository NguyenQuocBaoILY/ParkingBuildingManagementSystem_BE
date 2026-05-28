using QRCoder;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class QrCodeService : IQrCodeService
{
    public string GenerateBase64(string content)
    {
        var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(10); // 10px per module → ~250px image
        return Convert.ToBase64String(bytes);
    }
}
