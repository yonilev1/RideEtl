using Confluent.Kafka;
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

public class StationInformationService : BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private HttpClient _client;
    private readonly KafkaProducerService _producer;
    private readonly string  _topic = "bike.station-information";
    private readonly ILogger<StationInformationService> _logger;

    public StationInformationService(IHttpClientFactory httpFactory,
        KafkaProducerService producer,
        ILogger<StationInformationService> logger)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _producer = producer;
        _client = _httpFactory.CreateClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var response = await _client.GetAsync("https://gbfs.lyft.com/gbfs/2.3/bkn/en/station_information.json");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    StationInformation diserilized = JsonSerializer.Deserialize<StationInformation>(content);
                    if (IsValidData(diserilized))
                        await _producer.SendAsync(_topic, diserilized);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching status: {ex.Message}");
                Console.WriteLine($"Error fetching status: {ex.Message}");
            }
        }
    }

    private bool IsValidData(StationInformation data)
    {
        if (string.IsNullOrEmpty(data.StationId))
        { 
            _logger.LogError($"Data rejected - Station {data.Name} Id does not exist.");
            return false;
        }

        if (data.Lat > 90 || data.Lat < -90)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} Lat not in range -90 To 90.");
            return false;
        }

        if (data.Lon > 180 || data.Lon < -180)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} Lon not in range -180 To 180.");
            return false;
        }

        if (data.Capacity < 0)
        {
            _logger.LogError($"Data rejected - Station {data.StationId} Capacity cant be negitive.");
            return false;
        }

        return true;
    }
}
