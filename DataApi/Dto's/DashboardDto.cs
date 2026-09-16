namespace DataApi.Dto_s;

public class DashboardDto
{
    public int TotalStations { get; set; }
    public int EmptyStations { get; set; }
    public int FullStations { get; set; }
    public int lowAvailabilityStations { get; set; }
    public int outOfServiceStations { get; set; }
}
