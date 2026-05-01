using System.Threading.Tasks;

namespace CoffeeMachine.Services;

public interface IWeatherService
{
    /// <summary>
    /// Gets the current temperature in Celsius for the configured location.
    /// </summary>
    Task<double?> GetCurrentTemperatureAsync();
}
