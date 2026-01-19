﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodoFunc.Migrator;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Register the migration runner
builder.Services.AddTransient<MigrationRunner>();

var host = builder.Build();

// Run migrations
var migrationRunner = host.Services.GetRequiredService<MigrationRunner>();
await migrationRunner.RunMigrationsAsync();

Console.WriteLine("Database migrations completed successfully.");

