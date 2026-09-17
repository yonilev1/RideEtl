

using DataApi.Dto_s;
using DataApi.Dtos;
using DataApi.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IGetDataService _repo;
    private readonly ILogger<StationsController> _logger;
    private readonly HttpClient _client;

    public StationsController(IGetDataService repo, ILogger<StationsController> logger, HttpClient client)
    {
        _repo = repo;
        _logger = logger;
        _client = client;
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

    [HttpGet("location/reverse")]
    public async Task<ActionResult<string>> GetReverseLocation(double lat, double lon)
    {
        //var response = await _client.GetAsync($"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}");

        // 1. מייצרים את הבקשה באופן ידני
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}");

        // 2. מוסיפים את הכותרת שדורשת OpenStreetMap כדי לא לחסום אותך
        request.Headers.Add("User-Agent", "BikeSharingPlatform/1.0");

        // 3. שולחים את הבקשה עם ה-client הקיים שלך
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var jsonResult = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonResult);
        string displayName = document.RootElement.GetProperty("display_name").GetString();
        return Ok(displayName); 
    }
}