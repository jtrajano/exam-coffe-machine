using CoffeeMachine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<CoffeeMachine.Providers.IDateTimeProvider, CoffeeMachine.Providers.DateTimeProvider>();
builder.Services.AddSingleton<CoffeeMachine.Services.ICallCounterService, CoffeeMachine.Services.CallCounterService>();
builder.Services.AddSingleton<CoffeeMetrics>();
builder.Services.AddHttpClient<CoffeeMachine.Services.IWeatherService, CoffeeMachine.Services.OpenWeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();

public partial class Program { }
