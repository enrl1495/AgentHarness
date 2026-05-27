using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.IO;

namespace AgentHarness.Core.Memory;

/// <summary>
/// Default implementation of <see cref="ISqliteConnectionFactory"/>.
/// Creates connections to the shared AgentHarness database.
/// </summary>
public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteConnectionFactory> _logger;

    public SqliteConnectionFactory(ILogger<SqliteConnectionFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AgentHarness");
        if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);

        var dbPath = Path.Combine(dbFolder, "agentharness.db");
        _connectionString = $"Data Source={dbPath}";
        
        _logger.LogInformation("SQLite connection factory initialized. Database: {DbPath}", dbPath);
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
