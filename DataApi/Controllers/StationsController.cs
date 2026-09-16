

using DataApi.Dtos;
using DataApi.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IGetDataService _repo;
    private readonly ILogger<StationsController> _logger;

    public StationsController(IGetDataService repo, ILogger<StationsController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FullStationDto>>> FilterStations(int? minAvailableBikes,
        int? isRenting, int? isRerurning)
    {
        return Ok(await _repo.FilterStation(minAvailableBikes, isRenting, isRerurning));
    }

    [HttpGet("{stationId}")]
    public async Task<ActionResult<FullStationDto?>> GetStationById(string stationId)
    {
        var station = await _repo.GetStationById(stationId);

        if (station == null)
            return NotFound();
        return Ok(station);
    }
}