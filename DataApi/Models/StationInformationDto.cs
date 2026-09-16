using System.Text.Json.Serialization;

namespace DataApi.Models;

public class StationInformationDto
{
    public string StationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public int Capacity { get; set; }
}

