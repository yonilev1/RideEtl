using Confluent.Kafka;
using Consumer.Handlers;
using Consumer.Models;
using DnsClient.Internal;
using DnsClient.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;


namespace Consumer.Consumers;

public class StationStatusConsumer : BackgroundService
{
    private readonly ILogger<StationStatusConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _bootstrapServer;
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;

    public StationStatusConsumer(ILogger<StationStatusConsumer> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _bootstrapServer = configuration["Kafka:BootstrapServer"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:StationStatusTopic"] ?? "bike.station-status";

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServer,
            GroupId = "station-status-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        _logger.LogInformation($"Started consuming from topic: {_topic}");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consume = _consumer.Consume();
                    if (consume == null || consume.Message.Value == null)
                        continue;

                    var dto = JsonSerializer.Deserialize<StationStatus>(consume.Message.Value);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<StationStatusHandler>();
                        await handler.HandleAsync(dto);
                    }


                    _logger.LogInformation($"Successfully processed status for station {dto.StationId}");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Kafka consume error: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"JSON Deserialization error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error processing message: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumption canceled by the host.");
        }
        finally
        {
            _consumer.Close();
        }
    }
}
