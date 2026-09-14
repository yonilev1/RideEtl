using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Services;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var bootstrapServer = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.AddSingleton<KafkaProducerService>(provider =>
        new KafkaProducerService(bootstrapServer));

        services.AddHostedService<StationInformationService>();
        services.AddHostedService<StationStatusService>();
        services.AddHostedService<VehicleTypesService>();
    }).Build();

await host.RunAsync();