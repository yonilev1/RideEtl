using DataApi.Data;
using DataApi.Dtos;
using DataApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;
using System.Diagnostics;
using StackExchange.Redis;
using System.Text.Json;
namespace DataApi.Service;

public class GetDataService : IGetDataService
{
    private readonly PiplineDbContext _context;
    private readonly ILogger<GetDataService> _logger;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _mongoDb;
    private readonly StackExchange.Redis.IDatabase _db;

    public GetDataService(PiplineDbContext context,
        ILogger<GetDataService> logger,
        IMongoClient client,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _client = client;
        _mongoDb = _client.GetDatabase("stationsStatusDb");
        _db = redis.GetDatabase();
    }

    public async Task<IEnumerable<FullStationDto>> FilterStation(int? minAvailableBikes,
        int? isRenting, int? isRerurning)
    {
        var stationInfo = await _context.StationInfo.ToListAsync();

        var fullStations = new List<FullStationDto>();
        
        foreach(var info in stationInfo)
        {
            var redisValue = await _db.StringGetAsync(info.StationId);

            if(redisValue.HasValue)
            {
                var status = JsonSerializer.Deserialize<StationStatusDto>(redisValue);
                bool matches = true;

                if (minAvailableBikes.HasValue && status.NumBikesAvailable < minAvailableBikes) matches = false;
                if (isRenting.HasValue && status.IsRenting != isRenting) matches = false;
                if (isRerurning.HasValue && status.IsReturning != isRerurning) matches = false;

                if (matches)
                {
                    fullStations.Add(new FullStationDto
                    {
                        StationId = info.StationId,
                        Name = info.Name,
                        Lat = info.Lat,
                        Lon = info.Lon,
                        Capacity = info.Capacity,
                        NumBikesAvailable = status.NumBikesAvailable,
                        NumDocksAvailable = status.NumDocksAvailable,
                        IsRenting = status.IsRenting,
                        IsReturning = status.IsReturning
                    });
                }
            }
        }
        return fullStations;
    }

    public async Task<FullStationDto?> GetStationById(string stationId)
    {
        var builder = Builders<StationStatusDto>.Filter;
        var filter = builder.Empty;
        filter &= builder.Eq(s => s.StationId, stationId);
        var coll = _mongoDb.GetCollection<StationStatusDto>("stationCollection");
        var filteredStatusById = await coll.Find(filter)
            .SortByDescending(s => s.LastReported)
            .FirstOrDefaultAsync();

        var filteredStationById = await _context.StationInfo.FirstOrDefaultAsync(s => s.StationId == stationId);
        if (filteredStationById == null || filteredStatusById == null)
            return null;

        FullStationDto station = new FullStationDto
        {
            StationId = filteredStationById.StationId,
            Name = filteredStationById.Name,
            Capacity = filteredStationById.Capacity,
            Lon = filteredStationById.Lon,
            Lat = filteredStationById.Lat,
            NumBikesAvailable = filteredStatusById.NumBikesAvailable,
            NumDocksAvailable = filteredStatusById.NumDocksAvailable,
            IsRenting = filteredStatusById.IsRenting,
            IsReturning = filteredStatusById.IsReturning
        };
        return station;




    }
}
