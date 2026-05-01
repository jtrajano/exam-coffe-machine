# Coffee Machine API ☕

A production-grade .NET 10 Minimal API implementation designed for a senior technical assessment. This service provides a resilient, observable, and secure coffee brewing logic with external weather integration.

## 🚀 Key Features & Implementation

### 1. Robust Business Logic
- **April Fools Logic**: Returns `HTTP 418 I'm a teapot` on April 1st.
- **Maintenance Cycle**: Returns `HTTP 503 Service Unavailable` on every 5th request.
- **Persistence**: Uses **SQLite with Write-Ahead Logging (WAL)** for high-concurrency atomic counter tracking. Counts persist across application restarts.
- **ISO 8601 Compliance**: All response timestamps strictly follow ISO 8601 standards.

### 2. Advanced Weather Integration (Extra Credit)
- **Intelligent Brewing**: Integrates with OpenWeatherMap. If the temperature > 30°C, it automatically prepares "refreshing iced coffee" instead of "piping hot coffee."
- **Polly Resilience**: The weather client is protected by the **Microsoft Standard Resilience Handler**, including retries, circuit breaking, and timeouts.
- **Graceful Degradation**: If the weather service is unavailable or misconfigured, the API defaults to "piping hot" coffee to ensure continuous service.

### 3. Security & Hardening
- **API Key Authentication**: Protected via a custom `IEndpointFilter`. Requires a valid `X-API-Key` header for all requests.
- **Rate Limiting**: Integrated fixed-window rate limiter (10 req/min) to prevent abuse.
- **Security Headers**: Enforces `X-Content-Type-Options`, `X-Frame-Options`, and other safety headers.
- **Global Exception Handling**: Centralized `ProblemDetails` middleware ensures consistent, non-leaking error responses.

### 4. Observability
- **Structured Logging**: Uses modern structured log templates for better searchability in tools like Azure Application Insights or ELK.
- **Custom Metrics**: Exposes `CoffeeMachine.Brewed` counters via `System.Diagnostics.Metrics`.

---

## 🛠️ Getting Started

### Local Development
1. **Prerequisites**: .NET 10 SDK.
2. **Secrets Configuration**: The project uses `.NET User Secrets` for sensitive keys.
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Weather:ApiKey" "your_actual_key"
   ```
3. **Execution**:
   ```bash
   dotnet run --project src/CoffeeMachine
   ```

### 🧪 Testing
The project includes a robust integration test suite with **full isolation**. Every test runs with a unique, temporary SQLite database to ensure no state bleed.

```bash
dotnet test
```

---

## 📖 API Documentation & Manual Testing

### Interactive Documentation (Scalar)
In .NET 10, we've moved to **Scalar** for a more modern and responsive API reference.
- **URL**: `http://localhost:<port>/scalar/v1`

### Authentication for Examiners
To test the protected endpoints, use the following **Default Key**:
- **Header**: `X-API-Key`
- **Value**: `C0ffee-Key-2024`

In the Scalar UI, look for the **Security/Authorize** section to enter the key once for all requests.

---

## 🏗️ Technical Architecture
- **Clean Code**: Logic is decoupled into `Services`, `Middleware`, `Providers`, and `Endpoints`.
- **TDD Ready**: Use of abstractions like `IDateTimeProvider` ensures that time-sensitive logic can be verified deterministically.
- **Performance**: Asynchronous operations throughout to prevent thread starvation during database or network I/O.
