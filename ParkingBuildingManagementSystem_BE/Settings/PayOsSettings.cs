namespace ParkingBuildingManagementSystem_BE.Settings;

public class PayOsSettings
{
    public const string SectionName = "PayOS";
    public string ClientId { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ChecksumKey { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string CancelUrl { get; set; } = null!;
}
