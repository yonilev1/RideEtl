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

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string connectionString = configuration.GetConnectionString("DefaultConnection");

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // בלוק 1: הבאת הגדרות (אם צריך לשלוף ערכים ספציפיים)
        var config = context.Configuration;

        // בלוק 2: תשתיות 
        // services.AddDbContext<...
        services.AddDbContext<PiplineDbContext>(options =>
        options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString)));
        // services.AddSingleton<IConnectionMultiplexer>(...
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect("localhost:6379"));
        // MongoDB Registration...
        services.AddSingleton<IMongoClient>(
            new MongoClient("mongodb://root:root@localhost:27017/"));

        // בלוק 3: הנדלרים
        // services.AddScoped<StationStatusHandler>();
        services.AddScoped<StationInformationHandler>();
        services.AddScoped<StationStatusHandler>();
        services.AddScoped<VehicleTypesHandler>();
        // ...

        // בלוק 4: קונסומרים
        // services.AddHostedService<StationStatusConsumer>();
        services.AddHostedService<StationInformationConsumer>();
        services.AddHostedService<StationStatusConsumer>();
        services.AddHostedService<VehicleTypesConsumer>();
        // ...
    })
    .Build();

using(var scoped = host.Services.CreateScope())
{
    var dbContext = scoped.ServiceProvider.GetRequiredService<PiplineDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();