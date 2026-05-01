namespace CoffeeMachine.Services;

public interface ICallCounterService
{
    Task<int> IncrementAndGetAsync();
}
