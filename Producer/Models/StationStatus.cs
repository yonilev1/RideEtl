using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Producer.Models;

public class StationStatus
{
    [JsonPropertyName("data")]
    public StationStatusDataDto Data { get; set; }
}

public class StationStatusDataDto
{
    [JsonPropertyName("stations")]
    public List<StationStatusDto> Stations { get; set; } = new List<StationStatusDto>();
}

public class StationStatusDto
{
    [JsonPropertyName("station_id")]
    public string StationId { get; set; } = string.Empty;

    [JsonPropertyName("num_bikes_available")]
    public int NumBikesAvailable { get; set; }

    [JsonPropertyName("num_bikes_disabled")]
    public int NumBikesDisabled { get; set; }

    [JsonPropertyName("num_docks_available")]
    public int NumDocksAvailable { get; set; }

    [JsonPropertyName("num_docks_disabled")]
    public int NumDocksDisabled { get; set; }

    [JsonPropertyName("is_installed")]
    public int IsInstalled { get; set; }

    [JsonPropertyName("is_renting")]
    public int IsRenting { get; set; }

    [JsonPropertyName("is_returning")]
    public int IsReturning { get; set; }

    [JsonPropertyName("last_reported")]
    public long LastReported { get; set; }

    [JsonPropertyName("vehicle_types_available")]
    public List<VehicleTypeAvailableDto>? VehicleTypesAvailable { get; set; }

    [JsonPropertyName("num_ebikes_available")]
    public int NumEbikesAvailable { get; set; }
}

public class VehicleTypeAvailableDto
{
    [JsonPropertyName("vehicle_type_id")]
    public string VehicleTypeId { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}