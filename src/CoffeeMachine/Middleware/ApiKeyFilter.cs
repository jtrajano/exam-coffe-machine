namespace CoffeeMachine.Middleware;

public class ApiKeyFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            return Results.Unauthorized();
        }

        var apiKey = _configuration["Security:ApiKey"] ?? "C0ffee-Key-2024";

        if (!apiKey.Equals(extractedApiKey))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
