using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Consumer.Models;

public class VehicleType
{
    public string VehicleTypeId { get; set; } = string.Empty;
    public string FormFactor { get; set; } = string.Empty;
    public string PropulsionType { get; set; } = string.Empty;
    public double? MaxRangeMeters { get; set; }
}

