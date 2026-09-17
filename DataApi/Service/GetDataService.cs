using DataApi.Data;
using DataApi.Dto_s;
using DataApi.Dtos;
using DataApi.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
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
    private readonly IMongoCollection<StationStatusDto> _collection;

    public GetDataService(PiplineDbContext context,
        ILogger<GetDataService> logger,
        IMongoClient client,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _client = client;
        _mongoDb = _client.GetDatabase("stationsStatusDb");
        _collection = _mongoDb.GetCollection<StationStatusDto>("stationCollection");
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
        var filteredStatusById = await _collection.Find(filter)
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

    public async Task<FullStationsStatusDto?> GetStationStatusById(string stationId)
    {
        var builder = Builders<StationStatusDto>.Filter;
        var filter = builder.Empty;
        filter &= builder.Eq(s => s.StationId, stationId);
        var filteredStatusById = await _collection.Find(filter)
            .SortByDescending(s => s.LastReported)
            .FirstOrDefaultAsync();

        if (filteredStatusById == null)
            return null;

        FullStationsStatusDto status = new FullStationsStatusDto
        {
            StationId = filteredStatusById.StationId,
            NumBikesAvailable = filteredStatusById.NumBikesAvailable,
            NumDocksAvailable = filteredStatusById.NumDocksAvailable,
            IsRenting = filteredStatusById.IsRenting,
            IsReturning = filteredStatusById.IsReturning,
            LastReported = filteredStatusById.LastReported
        };
        return status;
    }

    public async Task<IEnumerable<GetByTimeDto>> GetStatusHistory(string stationId, DateTime? from,
        DateTime? to, int? limit)
    {
        List<GetByTimeDto> statusHistry = new List<GetByTimeDto>();
        var builder = Builders<StationStatusDto>.Filter;
        var filter = builder.Empty;

        filter &= builder.Eq(s => s.StationId, stationId);

        if (from.HasValue)
        {
            long longFrom = ((DateTimeOffset)from.Value).ToUnixTimeSeconds();
            filter &= builder.Gte(s => s.LastReported, longFrom);
        }
        if(to.HasValue)
        {
            long longTo = ((DateTimeOffset)to.Value).ToUnixTimeSeconds();
            filter &= builder.Lte(s => s.LastReported, longTo);
        }

        var query = _collection.Find(filter);
        
        if (limit.HasValue)
            query = query.Limit(limit.Value);

        var result = await query.ToListAsync();

        foreach(var res in result)
        {
            statusHistry.Add(
                new GetByTimeDto
                {
                    TimeStamp = res.LastReported,
                    NumBikesAvailable = res.NumBikesAvailable,
                    NumDocksAvailable = res.NumDocksAvailable
                });
        }
        return statusHistry;
    }

    public async Task<DashboardDto> GetSystemDashboard()
    {
        var stationIds = await _context.StationInfo.Select(s => s.StationId).ToListAsync();

        var redisKeys = stationIds.Select(id => (RedisKey)id).ToArray();

        var redisValues = await _db.StringGetAsync(redisKeys);

        var currentStatus = new List<StationStatusDto>();

        foreach(var val in redisValues)
        {
            if(val.HasValue)
            {
                var status = JsonSerializer.Deserialize<StationStatusDto>(val);
                currentStatus.Add(status);
            }
        }

        var Dashboard = new DashboardDto
        {
            TotalStations = _context.StationInfo.Count(),
            EmptyStations = currentStatus.Count(s => s.NumBikesAvailable == 0),
            FullStations = currentStatus.Count(s => s.NumDocksAvailable == 0),
            lowAvailabilityStations = currentStatus.Count(s => s.NumBikesAvailable < 10 && s.NumBikesAvailable > 0),
            outOfServiceStations = currentStatus.Count(s => s.IsRenting == 0 && s.IsReturning == 0)
        };
        return Dashboard;
    }
}
