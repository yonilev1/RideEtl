using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

}
