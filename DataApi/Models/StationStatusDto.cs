using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataApi.Models;

public class StationStatusDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("station_id")]
    public string StationId { get; set; } = string.Empty;

    [BsonElement("num_bikes_available")]
    public int NumBikesAvailable { get; set; }

    [BsonElement("num_docks_available")]
    public int NumDocksAvailable { get; set; }

    [BsonElement("is_renting")]
    public int IsRenting { get; set; }

    [BsonElement("is_returning")]
    public int IsReturning { get; set; }

    [BsonElement("last_reported")]
    public long LastReported { get; set; }
}
