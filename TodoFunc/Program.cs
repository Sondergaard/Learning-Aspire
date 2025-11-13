using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TodoFunc;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();

// Run migrations before starting the host
MigrationRunner.RunMigrations(builder.Configuration);

builder.Services.AddAzureClients(azure =>
{
    var connectionString = builder.Configuration.GetValue<string>("myservicebus") ?? throw new ArgumentNullException("myservicebus");

    azure.AddServiceBusClient(connectionString);
});

builder.Build().Run();