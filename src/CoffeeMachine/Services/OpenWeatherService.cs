using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoffeeMachine.Services;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<double?> GetCurrentTemperatureAsync()
    {
        var apiKey = _configuration["Weather:ApiKey"];
        var city = _configuration["Weather:City"] ?? "Sydney";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Weather API Key is missing. Skipping weather check.");
            return null;
        }

        try
        {
            // API returns temperature in Celsius because of units=metric
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(url);
            
            return response?.Main?.Temp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data from OpenWeatherMap.");
            return null;
        }
    }

    // Internal DTOs for OpenWeatherMap API response
    private class OpenWeatherResponse
    {
        public MainData? Main { get; set; }
    }

    private class MainData
    {
        public double Temp { get; set; }
    }
}
