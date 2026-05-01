using System.Threading;

namespace CoffeeMachine.Services;

public class CallCounterService : ICallCounterService
{
    private int _count = 0;

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref _count);
    }
}
