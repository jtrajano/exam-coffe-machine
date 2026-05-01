using CoffeeMachine.Endpoints;
using CoffeeMachine.Telemetry;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Core Services
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ICallCounterService, CallCounterService>();
builder.Services.AddSingleton<CoffeeMetrics>();

// Register Weather Integration (Phase 3)
builder.Services.AddHttpClient<IWeatherService, OpenWeatherService>();

var app = builder.Build();

// Map Endpoints
app.MapCoffeeEndpoints();

app.Run();

// Expose for Integration Tests
public partial class Program { }
