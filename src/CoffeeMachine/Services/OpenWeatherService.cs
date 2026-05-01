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
            _logger.LogWarning("Weather API Key is missing for city {City}. Skipping weather-based logic.", city);
            return null;
        }

        try
        {
            _logger.LogInformation("Fetching current temperature for {City}.", city);
            
            // API returns temperature in Celsius because of units=metric
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(url);
            
            var temp = response?.Main?.Temp;
            if (temp.HasValue)
            {
                _logger.LogInformation("Successfully retrieved temperature: {Temp}°C for {City}.", temp, city);
            }
            else
            {
                _logger.LogWarning("Weather API returned successfully but temperature data was missing for {City}.", city);
            }

            return temp;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching weather for {City}. Status Code: {StatusCode}", city, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather data for {City}.", city);
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
