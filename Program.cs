using CoffeeMachine.Endpoints;
using CoffeeMachine.Telemetry;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Core Services
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ICallCounterService, CallCounterService>();
builder.Services.AddSingleton<CoffeeMetrics>();

// Register Weather Integration (Phase 3)
builder.Services.AddHttpClient<IWeatherService, OpenWeatherService>();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty; // Serve Swagger at the app root
});

// Map Endpoints
app.MapCoffeeEndpoints();

app.Run();

// Expose for Integration Tests
public partial class Program { }
