namespace ParkingBuildingManagementSystem_BE.Settings;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public string SmtpUser { get; set; } = null!;
    public string SmtpPass { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = null!;

    // Base URL dùng để tạo link xác minh email trong nội dung email
    public string BaseUrl { get; set; } = null!;
}
