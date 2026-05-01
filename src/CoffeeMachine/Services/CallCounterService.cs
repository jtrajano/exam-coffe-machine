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
            CREATE TABLE IF NOT EXISTS Counters (
                Id INTEGER PRIMARY KEY,
                Value INTEGER
            );
            INSERT OR IGNORE INTO Counters (Id, Value) VALUES (1, 0);
        ";
        command.ExecuteNonQuery();
    }

    public int IncrementAndGet()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = 
        @"
            UPDATE Counters SET Value = Value + 1 WHERE Id = 1;
            SELECT Value FROM Counters WHERE Id = 1;
        ";

        var result = command.ExecuteScalar();
        transaction.Commit();

        var count = Convert.ToInt32(result);
        _metrics.Brewed();
        return count;
    }
}
