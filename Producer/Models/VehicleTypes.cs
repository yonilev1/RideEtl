using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Producer.Models;

public class VehicleType
{
    [JsonPropertyName("data")]
    public VehicleTypeDataDto Data { get; set; } = null!;
}

public class VehicleTypeDataDto
{
    [JsonPropertyName("vehicle_types")]
    public List<VehicleTypesDto> VehicleTypes { get; set; } = new List<VehicleTypesDto>();
}


public class VehicleTypesDto
{
    [JsonPropertyName("vehicle_type_id")]
    public string VehicleTypeId { get; set; } = string.Empty;

    [JsonPropertyName("form_factor")]
    public string FormFactor { get; set; } = string.Empty;

    [JsonPropertyName("propulsion_type")]
    public string PropulsionType { get; set; } = string.Empty;

    [JsonPropertyName("max_range_meters")]
    public double? MaxRangeMeters { get; set; }
}
