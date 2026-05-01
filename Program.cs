using CoffeeMachine.Endpoints;
using CoffeeMachine.Telemetry;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    });
});

// Register Core Services
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ICallCounterService, CallCounterService>();
builder.Services.AddSingleton<CoffeeMetrics>();

// Register Weather Integration (Phase 3)
builder.Services.AddHttpClient<IWeatherService, OpenWeatherService>();

var app = builder.Build();

// Use Rate Limiting
app.UseRateLimiter();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Serve Swagger at the app root
    });
}

// Map Endpoints
app.MapCoffeeEndpoints();

app.Run();

// Expose for Integration Tests
public partial class Program { }
