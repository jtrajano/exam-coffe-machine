# Coffee Machine API

A .NET 10 Minimal API implementation for a technical assessment. The project provides a `/brew-coffee` endpoint with specific business logic for error handling, state persistence, and external weather integration.

## Implementation Details

### Core Requirements

- **HTTP 418**: Returns "I'm a teapot" on April 1st.
- **HTTP 503**: Returns "Service Unavailable" every 5th call.
- **Persistence**: Unlike a standard in-memory counter, this implementation uses a local SQLite database (`coffee.db`) to track the request count. This ensures the 5th-call logic remains accurate even if the application process restarts.
- **ISO 8601**: Responses include a formatted timestamp as required.

### Extra Credit: Weather Integration

The API integrates with OpenWeatherMap. When the temperature at the configured location exceeds 30°C, the response message switches from "piping hot" to "refreshing iced coffee." The implementation is resilient; if the API key is missing or the external service is down, it fails gracefully back to the default hot coffee message.

## Project Structure

The project is organized by concern to ensure maintainability:

- `/Endpoints`: Minimal API route mapping and handlers.
- `/Services`: Business logic for call counting and weather integration.
- `/Models`: DTOs used for API responses.
- `/Providers`: Infrastructure abstractions (e.g., `IDateTimeProvider` for testability).
- `/Telemetry`: Custom metrics using `System.Diagnostics.Metrics`.

## Getting Started

### Local Setup

1. Ensure .NET 10 SDK is installed.
2. **Security**: This project uses `.NET User Secrets` to avoid exposing API keys. To add your OpenWeatherMap key, run:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Weather:ApiKey" "your_actual_key"
   ```

### Execution

```bash
dotnet run
```

Once running, the **Swagger UI** is available at the root URL (e.g., `http://localhost:5000/`) for interactive testing.

### Testing

Run the integration test suite via the CLI:

```bash
dotnet test
```

The tests use a `WebApplicationFactory` and mock providers to verify logic across specific dates, request volumes, and temperature thresholds.

## Monitoring

The application exposes custom metrics that can be monitored using `dotnet-counters`:

```bash
dotnet-counters monitor -n CoffeeMachine --counters CoffeeMachine
```

## Technical Choices

- **Minimal APIs**: Used for better performance and a cleaner entry point.
- **Rate Limiting**: Implemented a fixed-window rate limiter (10 requests/min) to protect the API from automated abuse and ensure service stability.
- **DI Abstractions**: The use of `IDateTimeProvider` allows for deterministic testing of date-based logic (April Fools) without hacking the system clock.
- **Transactions**: SQLite updates are wrapped in transactions to ensure thread-safety and data integrity during high-concurrency requests.
