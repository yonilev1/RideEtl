using Consumer.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Consumer.Handlers;

public class StationStatusHandler
{
    private readonly IDatabase _db;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _mongoDb;

    public StationStatusHandler(IConnectionMultiplexer redis, IMongoClient client)
    {
        _client = client;
        _mongoDb = _client.GetDatabase("stationsStatusDb");
        _db = redis.GetDatabase();
    }

    public async Task HandleAsync(StationStatus dto)
    {
        var entity = new StationStatusDto
        {
            StationId = dto.StationId,
            NumBikesAvailable = dto.NumBikesAvailable,
            NumDocksAvailable = dto.NumDocksAvailable,
            IsRenting = dto.IsRenting,
            IsReturning = dto.IsReturning,
            LastReported = dto.LastReported
        };

        string stringEntity = JsonSerializer.Serialize(entity);
        
        string? exists = await _db.StringGetAsync(entity.StationId);

        if (exists != stringEntity)
        {
            await _db.StringSetAsync(entity.StationId, stringEntity);
            var coll = _mongoDb.GetCollection<StationStatusDto>("stationCollection");
            await coll.InsertOneAsync(entity);
        }
    }
}
