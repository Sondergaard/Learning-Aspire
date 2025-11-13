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

// Wire up Azure Function with SQL Server connection
builder
    .AddAzureFunctionsProject<Projects.TodoFunc>(AppHosts.ToDoFunction)
    .WithReference(db)
    .WithReference(serviceBus)
    .WaitFor(db)
    .WaitFor(serviceBus);

builder.Build().Run();