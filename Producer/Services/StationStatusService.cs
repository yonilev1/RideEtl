using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
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

public class StationStatusService : BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private HttpClient _client;
    private readonly KafkaProducerService _producer;
    private readonly string _topic;
    private readonly ILogger<StationStatusService> _logger;

    public StationStatusService(IHttpClientFactory httpFactory,
        KafkaProducerService producer,
        ILogger<StationStatusService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _producer = producer;
        _client = _httpFactory.CreateClient("GbfsClient");
        _topic = configuration["Kafka:Topics:StationStatusTopic"] ?? "bike.station-status";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var response = await _client.GetAsync("station_status.json", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    StationStatus deserialized = JsonSerializer.Deserialize<StationStatus>(content);
                    foreach (StationStatusDto station in deserialized.Data.Stations)
                    {
                        if (IsValidData(station))
                            await _producer.SendAsync(_topic, station);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching status: {ex.Message}");
                Console.WriteLine($"Error fetching status: {ex.Message}");
            }
        }
    }

    private bool IsValidData(StationStatusDto data)
    {
        if (string.IsNullOrEmpty(data.StationId))
        {
            _logger.LogError($"Data rejected - Id does not exist.");
            return false;
        }

        if (data.NumDocksAvailable < 0)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} number of avalible docs shoulde be non negitive.");
            return false;
        }

        if (data.NumBikesAvailable < 0)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} number of avalible biks shoulde be non negitive.");
            return false;
        }

        if (data.NumEbikesAvailable < 0)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} number of avalible Ebiks shoulde be non negitive.");
            return false;
        }

        return true;
    }
}
