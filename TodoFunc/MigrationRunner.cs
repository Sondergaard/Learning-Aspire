using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TodoFunc;

public static class MigrationRunner
{
    public static void RunMigrations(IConfiguration config)
    {
        var connectionString = config["ConnectionStrings:ToDoDatabase"] ?? config.GetConnectionString("ToDoDatabase");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("SQL Server connection string not found.");

        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "Migrations");
        if (!Directory.Exists(migrationsPath)) return;

        foreach (var file in Directory.GetFiles(migrationsPath, "*.sql"))
        {
            var script = File.ReadAllText(file);
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand(script, conn);
            cmd.ExecuteNonQuery();
        }
    }
}