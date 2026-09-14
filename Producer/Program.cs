using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.AddSingleton<KafkaProducerService>(provider =>
        new KafkaProducerService("localhost:9092"));

        services.AddHostedService<StationInformationService>();
        services.AddHostedService<StationStatusService>();
        services.AddHostedService<VehicleTypesService>();
    }).Build();

await host.RunAsync();