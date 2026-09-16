namespace DataApi.Dto_s;

public class FullStationsStatusDto
{
    public string StationId { get; set; } = string.Empty;
    public int NumBikesAvailable { get; set; }
    public int NumDocksAvailable { get; set; }
    public int IsRenting { get; set; }
    public int IsReturning { get; set; }
    public long LastReported { get; set; }
}
