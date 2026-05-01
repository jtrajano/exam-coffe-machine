using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using CoffeeMachine.Telemetry;
using Microsoft.Extensions.Logging;

namespace CoffeeMachine.Services;

public class CallCounterService : ICallCounterService
{
    private readonly string _connectionString;
    private readonly CoffeeMetrics _metrics;
    private readonly ILogger<CallCounterService> _logger;
    private readonly string _dbName;

    public CallCounterService(IConfiguration configuration, CoffeeMetrics metrics, ILogger<CallCounterService> logger)
    {
        _connectionString = configuration.GetConnectionString("CoffeeDb") ?? "Data Source=coffee.db";
        _dbName = _connectionString.Split('=')[1];
        _metrics = metrics;
        _logger = logger;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = 
            @"
                PRAGMA journal_mode=WAL;
                CREATE TABLE IF NOT EXISTS Counters (
                    Id INTEGER PRIMARY KEY,
                    Value INTEGER
                );
                INSERT OR IGNORE INTO Counters (Id, Value) VALUES (1, 0);
            ";
            command.ExecuteNonQuery();
            _logger.LogInformation("Database {DbName} initialized successfully with WAL mode enabled.", _dbName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize the database {DbName}.", _dbName);
            throw;
        }
    }

    public async Task<int> IncrementAndGetAsync()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            
            var command = connection.CreateCommand();
            command.Transaction = transaction as SqliteTransaction;
            command.CommandText = 
            @"
                UPDATE Counters SET Value = Value + 1 WHERE Id = 1;
                SELECT Value FROM Counters WHERE Id = 1;
            ";

            var result = await command.ExecuteScalarAsync();
            await transaction.CommitAsync();

            var count = Convert.ToInt32(result);
            _logger.LogDebug("Counter incremented to {Count}.", count);
            _metrics.Brewed();
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while incrementing counter in database {DbName}.", _dbName);
            throw;
        }
    }
}
