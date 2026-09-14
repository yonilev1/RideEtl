using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer.Services;

public class KafkaProducerService
{
    private readonly string _bootstrapServer;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(string bootstrapServer)
    {
        _bootstrapServer = bootstrapServer;

        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServer
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task EnsureTopicExistsAsync(string topicName)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = _bootstrapServer
        };

        var admin = new AdminClientBuilder(config).Build();

        try
        {
            await admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });
            Console.WriteLine($"✓ Topic '{topicName}' created successfully.");
        }
        catch (CreateTopicsException ex)
        {
            if (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                Console.WriteLine($"✓ Topic '{topicName}' already exists.");
            else
                throw new Exception($"Failed to create topic: {ex.Results[0].Error.Reason}");
        }
    }

    public async Task<DeliveryResult<Null, string>>SendAsync<T>(string topicName, T data)
    {
        string value = JsonSerializer.Serialize(data);

        var message = new Message<Null, string>
        {
            Value = value
        };

        var result = await _producer.ProduceAsync(topicName, message);

        Console.WriteLine($"✓ Sent: Data {data} → Partition={result.Partition}, Offset ={result.Offset}");

        return result;
    }

    public void Dispose()
    {
        _producer.Flush();
        _producer.Dispose();
    }
}

