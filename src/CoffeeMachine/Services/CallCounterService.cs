using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using CoffeeMachine.Telemetry;

namespace CoffeeMachine.Services;

public class CallCounterService : ICallCounterService
{
    private readonly string _connectionString;
    private readonly CoffeeMetrics _metrics;

    public CallCounterService(IConfiguration configuration, CoffeeMetrics metrics)
    {
        _connectionString = configuration.GetConnectionString("CoffeeDb") ?? "Data Source=coffee.db";
        _metrics = metrics;
        InitializeDatabase();
    }

    private void InitializeDatabase()
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
    }

    public async Task<int> IncrementAndGetAsync()
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
        _metrics.Brewed();
        return count;
    }
}
