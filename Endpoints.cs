using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;

namespace CoffeeMachine;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/brew-coffee", BrewCoffeeAsync);
    }

    public static async Task<IResult> BrewCoffeeAsync(
        [FromServices] IDateTimeProvider dateTimeProvider,
        [FromServices] ICallCounterService callCounterService)
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

        string formattedDate = dateNow.ToString("yyyy-MM-ddTHH:mm:ss") + dateNow.ToString("zzz").Replace(":", "");
        return Results.Ok(new { message = "Your piping hot coffee is ready", prepared = formattedDate });
    }
}
