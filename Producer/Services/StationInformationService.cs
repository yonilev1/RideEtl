using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Producer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    private readonly string  _topic;
    private readonly ILogger<StationInformationService> _logger;

    public StationInformationService(IHttpClientFactory httpFactory,
        KafkaProducerService producer,
        ILogger<StationInformationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _producer = producer;
        _client = _httpFactory.CreateClient("GbfsClient");
        _topic = configuration["Kafka:Topics:StationsInformationTopic"] ?? "bike.station-information";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        do
        {
            try
            {
                var response = await _client.GetAsync("station_information.json", stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(stoppingToken);
                    var deserialized = JsonSerializer.Deserialize<StationInformation>(content);

                    if (deserialized?.Data?.Stations != null)
                    {
                        foreach (StationInformationFeedDto station in deserialized.Data.Stations)
                        {
                            if (IsValidData(station))
                            {
                                await _producer.SendAsync(_topic, station);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch data. Status code: {response.StatusCode}");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error while deserializing data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching station information.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private bool IsValidData(StationInformationFeedDto data)
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
