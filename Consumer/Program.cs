using Consumer.Consumers;
using Consumer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Consumer.Handlers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

//IConfiguration configuration = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false)
//    .Build();

//string connectionString = configuration.GetConnectionString("DefaultConnection");

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        string connectionString = config.GetConnectionString("DefaultConnection");
        var redisCon = config["Redis:ConnectionString"]!;
        var mongoCon = config["Mongo:ConnectionString"]!;

        services.AddDbContext<PiplineDbContext>(options =>
        options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString)));
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisCon));
        
        services.AddSingleton<IMongoClient>(
            new MongoClient(mongoCon));

        services.AddScoped<StationInformationHandler>();
        services.AddScoped<StationStatusHandler>();
        services.AddScoped<VehicleTypesHandler>();
        
        services.AddHostedService<StationInformationConsumer>();
        services.AddHostedService<StationStatusConsumer>();
        services.AddHostedService<VehicleTypesConsumer>();
    })
    .Build();

using(var scoped = host.Services.CreateScope())
{
    var dbContext = scoped.ServiceProvider.GetRequiredService<PiplineDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();