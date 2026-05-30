namespace ParkingBuildingManagementSystem_BE.Settings;

public class BookingSettings
{
    public const string SectionName = "Booking";

    public int CheckinGraceMinutes { get; set; } = 30;
}
