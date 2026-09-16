

using DataApi.Dto_s;
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

    [HttpGet("{stationId}/status")]
    public async Task<ActionResult<FullStationsStatusDto?>> GetStationStatusById(string stationId)
    {
        var status = await _repo.GetStationStatusById(stationId);

        if (status == null)
            return NotFound();
        return Ok(status);
    }

    [HttpGet("{stationId}/history")]
    public async Task<ActionResult<IEnumerable<GetByTimeDto>>> GetStatusHistory(string stationId, DateTime? from, DateTime? to, int? limit)
    {
        return Ok(await _repo.GetStatusHistory(stationId, from, to, limit));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetSystemDashboard()
    {
        return Ok(await _repo.GetSystemDashboard());
    }
}