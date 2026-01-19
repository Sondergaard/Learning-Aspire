using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TodoFunc.Migrator;

public class MigrationRunner
{
    private readonly string _connectionString;
    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(IConfiguration configuration, ILogger<MigrationRunner> logger)
    {
        _connectionString = configuration["ConnectionStrings:ToDoDatabase"] 
                            ?? configuration.GetConnectionString("ToDoDatabase")
                            ?? throw new InvalidOperationException("SQL Server connection string not found.");
        _logger = logger;
    }

    public async Task RunMigrationsAsync()
    {
        _logger.LogInformation("Starting database migrations...");

        await EnsureMigrationTableExistsAsync();

        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "Migrations");
        if (!Directory.Exists(migrationsPath))
        {
            _logger.LogWarning("Migrations directory not found at {Path}", migrationsPath);
            return;
        }

        var migrationFiles = Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(f => f)
            .ToList();

        if (migrationFiles.Count == 0)
        {
            _logger.LogInformation("No migration files found.");
            return;
        }

        foreach (var file in migrationFiles)
        {
            var migrationName = Path.GetFileName(file);
            
            if (await IsMigrationAppliedAsync(migrationName))
            {
                _logger.LogInformation("Migration {Migration} already applied, skipping.", migrationName);
                continue;
            }

            _logger.LogInformation("Applying migration: {Migration}", migrationName);
            
            var script = await File.ReadAllTextAsync(file);
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand(script, conn, (SqlTransaction)transaction);
                cmd.CommandTimeout = 300; // 5 minutes
                await cmd.ExecuteNonQueryAsync();

                await RecordMigrationAsync(conn, (SqlTransaction)transaction, migrationName);
                await transaction.CommitAsync();
                
                _logger.LogInformation("Migration {Migration} applied successfully.", migrationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply migration {Migration}", migrationName);
                await transaction.RollbackAsync();
                throw;
            }
        }

        _logger.LogInformation("All migrations completed successfully.");
    }

    private async Task EnsureMigrationTableExistsAsync()
    {
        const string createTableSql = @"
            IF OBJECT_ID(N'dbo.__MigrationHistory', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.__MigrationHistory (
                    MigrationName NVARCHAR(255) PRIMARY KEY,
                    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            END";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(createTableSql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<bool> IsMigrationAppliedAsync(string migrationName)
    {
        const string checkSql = "SELECT COUNT(1) FROM dbo.__MigrationHistory WHERE MigrationName = @MigrationName";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(checkSql, conn);
        cmd.Parameters.AddWithValue("@MigrationName", migrationName);
        
        var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    private async Task RecordMigrationAsync(SqlConnection conn, SqlTransaction transaction, string migrationName)
    {
        const string insertSql = "INSERT INTO dbo.__MigrationHistory (MigrationName) VALUES (@MigrationName)";

        await using var cmd = new SqlCommand(insertSql, conn, transaction);
        cmd.Parameters.AddWithValue("@MigrationName", migrationName);
        await cmd.ExecuteNonQueryAsync();
    }
}

