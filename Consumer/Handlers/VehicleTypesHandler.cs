using Consumer.Data;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Consumer.Handlers;

public class VehicleTypesHandler
{
    private readonly PiplineDbContext _context;
    private readonly ILogger<VehicleTypesHandler> _logger;

    public VehicleTypesHandler(PiplineDbContext context, ILogger<VehicleTypesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(VehicleTypes dto)
    {
        var entity = new VehicleTypeDto
        {
            VehicleTypeId = dto.VehicleTypeId,
            FormFactor = dto.FormFactor,
            PropulsionType = dto.PropulsionType,
            MaxRangeMeters = dto.MaxRangeMeters
        };

        try
        {
            var exists = await _context.VehicleTypes.FirstOrDefaultAsync(s => s.VehicleTypeId == entity.VehicleTypeId);

            if (exists == null)
            {
                _context.VehicleTypes.Add(entity);
            }
            else
            {
                exists.VehicleTypeId = entity.VehicleTypeId;
                exists.PropulsionType = entity.PropulsionType;
                exists.FormFactor = entity.FormFactor;
                exists.MaxRangeMeters = entity.MaxRangeMeters;
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }
}
