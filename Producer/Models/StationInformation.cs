using System.Text.Json.Serialization;

namespace Producer.Models;

public class StationInformation
{
    [JsonPropertyName("data")]
    public StationInformationDataDto Data { get; set; } = null!;
}

public class StationInformationDataDto
{
    [JsonPropertyName("stations")]
    public List<StationInformationFeedDto> Stations { get; set; } = new List<StationInformationFeedDto>();
}
public class StationInformationFeedDto
{
    [JsonPropertyName("station_id")]
    public string StationId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("region_id")]
    public string RegionId { get; set; } = string.Empty;

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("rental_uris")]
    public RentalUrisDto RentalUris { get; set; } = null!;
}

public class RentalUrisDto
{
    [JsonPropertyName("android")]
    public string Android { get; set; } = string.Empty;

    [JsonPropertyName("ios")]
    public string Ios { get; set; } = string.Empty;
}
