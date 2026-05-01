using CoffeeMachine.Endpoints;
using CoffeeMachine.Middleware;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;
using CoffeeMachine.Telemetry;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI & Scalar Configuration (.NET 10 Way)
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpClient<IWeatherService, OpenWeatherService>()
    .AddStandardResilienceHandler();

builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ICallCounterService, CallCounterService>();
builder.Services.AddSingleton<CoffeeMetrics>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseRateLimiter();

app.MapCoffeeEndpoints();

app.Run();
