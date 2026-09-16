using MongoDB.Bson.Serialization.Attributes;

namespace DataApi.Dtos;

public class FullStationDto
{
    public string StationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public int Capacity { get; set; }
    public int NumBikesAvailable { get; set; }
    public int NumDocksAvailable { get; set; }
    public string Status { get; set; } = string.Empty;
    public int IsRenting { get; set; }
    public int IsReturning { get; set; }
}
