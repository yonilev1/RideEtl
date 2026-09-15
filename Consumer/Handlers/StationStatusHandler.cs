using Consumer.Models;
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
    //private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public StationStatusHandler(IConnectionMultiplexer redis)
    {
        //_redis = ConnectionMultiplexer.Connect("localhost:6379"); ;
        _db = redis.GetDatabase();
    }

    public async Task ExecuteAsync(StationStatus dto)
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
            //TODO - store in mongo
        }
    }
}
