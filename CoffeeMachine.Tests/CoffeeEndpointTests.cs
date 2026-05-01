using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using CoffeeMachine.Providers;
using CoffeeMachine.Services;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Diagnostics.Metrics;

namespace CoffeeMachine.Tests;

public class CoffeeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IConfiguration _testConfig;
    private readonly CoffeeMetrics _testMetrics;

    public CoffeeEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _testConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                {"ConnectionStrings:CoffeeDb", $"Data Source=test_{Guid.NewGuid()}.db"}
            })
            .Build();

        var services = new ServiceCollection();
        services.AddMetrics();
        var meterFactory = services.BuildServiceProvider().GetRequiredService<IMeterFactory>();
        _testMetrics = new CoffeeMetrics(meterFactory);
    }

    [Fact]
    public async Task GetBrewCoffee_Returns200Ok_WithCorrectJson()
    {
        // Arrange
        var mockDate = new DateTimeOffset(2021, 2, 3, 11, 56, 24, TimeSpan.FromHours(9));
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDateTimeProvider>(new MockDateTimeProvider(mockDate));
                services.AddSingleton<ICallCounterService>(new CallCounterService(_testConfig, _testMetrics));
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/brew-coffee");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        Assert.NotNull(content);
        Assert.Equal("Your piping hot coffee is ready", content.message);
        Assert.Equal("2021-02-03T11:56:24+0900", content.prepared);
    }

    [Fact]
    public async Task GetBrewCoffee_OnApril1st_Returns418ImATeapot()
    {
        // Arrange
        var mockDate = new DateTimeOffset(2023, 4, 1, 10, 0, 0, TimeSpan.Zero);
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDateTimeProvider>(new MockDateTimeProvider(mockDate));
                services.AddSingleton<ICallCounterService>(new CallCounterService(_testConfig, _testMetrics));
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/brew-coffee");

        // Assert
        Assert.Equal((HttpStatusCode)418, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Empty(content);
    }

    [Fact]
    public async Task GetBrewCoffee_OnFifthCall_Returns503ServiceUnavailable()
    {
        // Arrange
        var mockDate = new DateTimeOffset(2023, 5, 1, 10, 0, 0, TimeSpan.Zero);
        // We need a shared counter instance across the 5 calls
        var sharedCounter = new CallCounterService(_testConfig, _testMetrics);
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDateTimeProvider>(new MockDateTimeProvider(mockDate));
                services.AddSingleton<ICallCounterService>(sharedCounter);
            });
        }).CreateClient();

        // Act & Assert
        for (int i = 1; i <= 4; i++)
        {
            var response = await client.GetAsync("/brew-coffee");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // 5th call
        var response5 = await client.GetAsync("/brew-coffee");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response5.StatusCode);
        Assert.Empty(await response5.Content.ReadAsStringAsync());

        // 6th call
        var response6 = await client.GetAsync("/brew-coffee");
        Assert.Equal(HttpStatusCode.OK, response6.StatusCode);
    }

    [Fact]
    public async Task GetBrewCoffee_PersistsCounterAcrossRestarts()
    {
        // Arrange
        var testDbName = $"test_persistence_{Guid.NewGuid()}.db";
        var testDb = $"Data Source={testDbName}";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                {"ConnectionStrings:CoffeeDb", testDb}
            })
            .Build();
        
        // Simulating 1st Application Start
        {
            var counter1 = new CallCounterService(config, _testMetrics);
            counter1.IncrementAndGet(); // count = 1
            counter1.IncrementAndGet(); // count = 2
        }

        // Simulating 2nd Application Start (Restart)
        {
            var counter2 = new CallCounterService(config, _testMetrics);
            var currentCount = counter2.IncrementAndGet(); // should be 3
            Assert.Equal(3, currentCount);
        }

        // Cleanup
        SqliteConnection.ClearAllPools();
        if (System.IO.File.Exists(testDbName)) System.IO.File.Delete(testDbName);
    }

    private class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now { get; }

        public MockDateTimeProvider(DateTimeOffset now)
        {
            Now = now;
        }
    }

    private record CoffeeResponse(string message, string prepared);
}
