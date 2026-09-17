using DataApi.Data;
using DataApi.Service;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<DataApi.GlobalErrorHandeling.ErrorHandeling>();

builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PiplineDbContext>(options =>
options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<IMongoClient>(
    new MongoClient("mongodb://root:root@localhost:27017/"));

builder.Services.AddScoped<IGetDataService, GetDataService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
ConnectionMultiplexer.Connect("localhost:6379"));


builder.Services.AddHttpClient("NominatimClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "BikeSharingPlatform/1.0");
}); var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
  
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
