using DbUp;
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
        _logger.LogInformation("Starting database migrations with DbUp...");

        // Ensure database exists
        EnsureDatabase.For.SqlDatabase(_connectionString);

        // Configure DbUp to run migrations from the Migrations folder
        var upgrader = DeployChanges.To
            .SqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .LogTo(new DbUpLogger(_logger))
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger.LogError(result.Error, "Database migration failed!");
            throw result.Error;
        }

        _logger.LogInformation("Database migrations completed successfully.");
        
        // Return completed task for async compatibility
        await Task.CompletedTask;
    }

    // Custom logger adapter to integrate DbUp with ILogger
    private class DbUpLogger : DbUp.Engine.Output.IUpgradeLog
    {
        private readonly ILogger _logger;

        public DbUpLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogDebug(string format, params object[] args)
        {
            _logger.LogDebug(format, args);
        }

        public void LogInformation(string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            _logger.LogWarning(format, args);
        }

        public void LogError(string format, params object[] args)
        {
            _logger.LogError(format, args);
        }

        public void LogError(Exception ex, string format, params object[] args)
        {
            _logger.LogError(ex, format, args);
        }

        public void LogTrace(string format, params object[] args)
        {
            _logger.LogTrace(format, args);
        }
    }
}

