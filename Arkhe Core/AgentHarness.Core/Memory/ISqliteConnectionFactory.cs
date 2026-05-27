using Microsoft.Data.Sqlite;

namespace AgentHarness.Core.Memory;

/// <summary>
/// Factory interface for creating SQLite connections.
/// Enables co-located database files with shared connection management.
/// </summary>
public interface ISqliteConnectionFactory
{
    /// <summary>
    /// Creates a new SQLite connection.
    /// </summary>
    /// <returns>A new SQLite connection instance.</returns>
    SqliteConnection CreateConnection();
}
