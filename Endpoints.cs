using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeMachine;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/brew-cofee", BrewCoffeAsync);
    }


    public static async Task<IResult> BrewCoffeAsync()
    {
        DateTime dateNow = DateTime.UtcNow;
        
        if(dateNow.Month == 4 && dateNow.Day == 1)
                return Results.StatusCode(StatusCodes.Status418ImATeapot);

        return Results.Ok(new { message= "Your piping hot coffee is ready", prepared= dateNow });
    }

}
