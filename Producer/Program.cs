using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Services;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs.log")
    .CreateLogger();


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string kafkaBootstrap = context.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        services.AddHttpClient("GbfsClient", client =>
        {
            client.BaseAddress = new Uri("https://gbfs.lyft.com/gbfs/2.3/bkn/en/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton(provider => new KafkaProducerService(kafkaBootstrap));

        services.AddHostedService<StationInformationService>();
        services.AddHostedService<StationStatusService>();
        services.AddHostedService<VehicleTypesService>();
    }).Build();

await host.RunAsync();