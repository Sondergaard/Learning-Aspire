# TodoFunc.Migrator

This is a dedicated database migration console application that handles all database schema changes and initialization for the Todo application.

## Purpose

Following security best practices, this migrator application separates database schema management (DDL operations) from the main application. This allows the TodoFunc application to run with minimal database privileges (only CRUD operations) while the migrator runs with elevated privileges during deployment/startup.

## Features

- **Migration Tracking**: Uses a `__MigrationHistory` table to track applied migrations
- **Idempotent Execution**: Safely re-runs without applying the same migration twice
- **Transaction Safety**: Each migration runs in a transaction and rolls back on failure
- **Ordered Execution**: Migrations are applied in alphabetical order by filename
- **Logging**: Comprehensive logging of migration status and errors

## Adding New Migrations

1. Create a new SQL file in the `Migrations` folder
2. Name it with a sequential number prefix (e.g., `002_AddUserColumn.sql`)
3. Write your migration SQL (use `IF OBJECT_ID` checks for idempotency where applicable)

Example:
```sql
-- 002_AddUserColumn.sql
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Todos') AND name = 'UserId')
BEGIN
    ALTER TABLE dbo.Todos ADD UserId INT NULL;
END
```

## How It Works

1. The migrator is registered in `AppHost.cs` as a project that runs before the main TodoFunc application
2. It connects to the database using the same connection string reference as TodoFunc
3. It ensures the `__MigrationHistory` table exists
4. It checks which migrations have already been applied
5. It applies any pending migrations in order
6. It exits once complete, allowing the main application to start

## Connection String

The migrator uses the same connection string configuration as the main application:
- In Aspire: Automatically injected via `.WithReference(db)`
- Configuration key: `ConnectionStrings:ToDoDatabase`

## Running Locally

The migrator runs automatically when you start the Aspire AppHost. To run it manually:

```bash
dotnet run --project TodoFunc.Migrator
```

Make sure the database is running and the connection string is configured in your environment or user secrets.

