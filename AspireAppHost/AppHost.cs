using AppHost;
var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder
    .AddAzureServiceBus("myservicebus")
    .RunAsEmulator(container =>
        container.WithLifetime(ContainerLifetime.Persistent));

serviceBus.AddServiceBusQueue("ToDoCreated");

// Add SQL Server resource
var sql = builder.AddSqlServer("sqlserver")
    .WithImage("mssql/server:2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("ToDoDatabase");

// Add Database Migrator - runs once at startup
var migrator = builder
    .AddProject<Projects.TodoFunc_Migrator>("todofunc-migrator")
    .WithReference(db)
    .WaitFor(db);

// Wire up Azure Function with SQL Server connection
builder
    .AddAzureFunctionsProject<Projects.TodoFunc>(AppHosts.ToDoFunction)
    .WithReference(db)
    .WithReference(serviceBus)
    .WaitFor(migrator) // Wait for migrations to complete
    .WaitFor(serviceBus);

builder.Build().Run();

