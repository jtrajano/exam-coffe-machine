using Microsoft.AspNetCore.Mvc;
using CoffeeMachine.Models;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;

namespace CoffeeMachine.Endpoints;

public static class CoffeeEndpoints
{
    public static void MapCoffeeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/brew-coffee", BrewCoffeeAsync);
    }

    public static async Task<IResult> BrewCoffeeAsync(
        [FromServices] IDateTimeProvider dateTimeProvider,
        [FromServices] ICallCounterService callCounterService,
        [FromServices] IWeatherService weatherService)
    {
        var dateNow = dateTimeProvider.Now;
        
        // Check April Fools' Day first
        if (dateNow.Month == 4 && dateNow.Day == 1)
        {
            return Results.StatusCode(StatusCodes.Status418ImATeapot);
        }

        // Increment counter
        int callCount = callCounterService.IncrementAndGet();
        
        // Every 5th call returns 503
        if (callCount % 5 == 0)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        // Check weather for extra credit
        var temperature = await weatherService.GetCurrentTemperatureAsync();
        var message = (temperature > 30) 
            ? "Your refreshing iced coffee is ready" 
            : "Your piping hot coffee is ready";

        string formattedDate = dateNow.ToString("yyyy-MM-ddTHH:mm:ss") + dateNow.ToString("zzz").Replace(":", "");
        return Results.Ok(new CoffeeResponse(message, formattedDate));
    }
}
