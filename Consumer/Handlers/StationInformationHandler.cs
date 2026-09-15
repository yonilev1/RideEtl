using Consumer.Data;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Consumer.Handlers;

public class StationInformationHandler
{
    private readonly PiplineDbContext _context;
    private readonly ILogger<StationInformationHandler> _logger;
    
    public StationInformationHandler(PiplineDbContext context, ILogger<StationInformationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(StationInformationFeedDto dto)
    {
        var entity = new StationInformationDto
        {
            StationId = dto.StationId,
            Name = dto.Name,
            Lat = dto.Lat,
            Lon = dto.Lon,
            Capacity = dto.Capacity
        };
        try
        {
            var exists = await _context.StationInfo.FirstOrDefaultAsync(s => s.StationId == entity.StationId);

            if (exists == null)
            {
                _context.StationInfo.Add(entity);
            }
            else
            {
                exists.StationId = entity.StationId;
                exists.Name = entity.Name;
                exists.Lat = entity.Lat;
                exists.Lon = entity.Lon;
                exists.Capacity = entity.Capacity;
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }
}
