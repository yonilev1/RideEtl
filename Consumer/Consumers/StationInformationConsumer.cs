using Confluent.Kafka;
using Consumer.Handlers;
using Consumer.Models;
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;



namespace Consumer.Consumers;

public class StationInformationConsumer : BackgroundService
{
    private readonly ILogger<StationInformationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _bootstrapServer;
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;
    
    public StationInformationConsumer(ILogger<StationInformationConsumer> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _bootstrapServer = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:StationsInformationTopic"] ?? "bike.station-information";

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServer,
            GroupId = "station-information-consumer-group",
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
                    var consume = _consumer.Consume(stoppingToken);
                    if (consume == null || consume.Message.Value == null)
                        continue;

                    var dto = JsonSerializer.Deserialize<StationInformationFeedDto>(consume.Message.Value);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<StationInformationHandler>();
                        await handler.HandleAsync(dto);
                    }

                    _logger.LogInformation($"Successfully processed info for station {dto.StationId}");
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
                    _logger.LogError(ex, "Error processing message in handler");
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
