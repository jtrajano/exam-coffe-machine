using System.Diagnostics.Metrics;

namespace CoffeeMachine.Telemetry;

public class CoffeeMetrics 
{
    public const string MeterName = "CoffeeMachine";
    private readonly Counter<int> _brewCount;

    public CoffeeMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _brewCount = meter.CreateCounter<int>("coffee.brews", "count", "Total number of coffee brews");
    }

    public void Brewed() => _brewCount.Add(1);
}
