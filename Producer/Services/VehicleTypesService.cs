using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Producer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer.Services;

public class VehicleTypesService: BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private HttpClient _client;
    private readonly KafkaProducerService _producer;
    private readonly string _topic = "bike.vehicle-types";
    private readonly ILogger<StationStatusService> _logger;

    public VehicleTypesService(IHttpClientFactory httpFactory,
        KafkaProducerService producer,
        ILogger<StationStatusService> logger)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _producer = producer;
        _client = _httpFactory.CreateClient("GbfsClient");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        do
        {
            try
            {
                var response = await _client.GetAsync("vehicle_types.json", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    VehicleType deserialized = JsonSerializer.Deserialize<VehicleType>(content);
                    foreach (VehicleTypesDto vehicle in deserialized.Data.VehicleTypes)
                    {
                        if (IsValidData(vehicle))
                            await _producer.SendAsync(_topic, vehicle);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching vehicle: {ex.Message}");
                Console.WriteLine($"Error fetching vehicle: {ex.Message}");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private bool IsValidData(VehicleTypesDto data)
    {
        if (string.IsNullOrEmpty(data.VehicleTypeId))
        {
            _logger.LogError($"Data rejected - Id does not exist.");
            return false;
        }

        if (data.MaxRangeMeters < 0)
        {
            _logger.LogError($"Data rejected - Vehiclr {data.VehicleTypeId} MaxRangeMeters shoulde be non negitive.");
            return false;
        }

        return true;
    }
}
