namespace CoffeeMachine.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }
}
