# Learning Aspire

A sample .NET Aspire project demonstrating cloud-native application orchestration with Azure Functions, SQL Server, and Azure Service Bus.

## 📋 Overview

This project showcases a Todo management system built with:

- **.NET Aspire** for cloud-native orchestration
- **Azure Functions** (.NET 9.0) for serverless HTTP endpoints
- **SQL Server** for data persistence
- **Azure Service Bus** for async messaging
- **OpenTelemetry** for observability

## 🏗️ Architecture

### Projects

- **AspireAppHost** - Application orchestration and resource management
- **AspireServiceDefaults** - Shared service defaults (health checks, telemetry, service discovery)
- **TodoFunc** - Azure Functions app with HTTP triggers and Service Bus integration
- **TodoFuncEndpointTests** - Integration tests for the Azure Functions

### Components

```text
┌─────────────────┐
│  Aspire AppHost │
└────────┬────────┘
         │
         ├─── SQL Server (containerized)
         │    └─── ToDoDatabase
         │
         ├─── Azure Service Bus (emulator)
         │    └─── ToDoCreated Queue
         │
         └─── Azure Functions (TodoFunc)
              ├─── HTTP: Create Todo
              ├─── HTTP: List Todos
              └─── ServiceBus: OnToDoCreated
```

## 🚀 Features

### HTTP Endpoints

- **POST /api/Create** - Create a new todo item
  - Stores the todo in SQL Server
  - Publishes a `ToDoCreated` event to Service Bus
  
- **POST /api/List** - Retrieve all todo items

### Event Processing

- **OnToDoCreated** - Service Bus trigger that processes todo creation events
  - Demonstrates async message processing
  - Logs event details

### Database Management

- Automatic migration execution on startup
- SQL Server running in a container with persistent lifetime
- Simple schema with `Todos` table

## 🛠️ Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)

## 🏃 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Sondergaard/Learning-Aspire.git
cd learning-aspire
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Run the Application

```bash
dotnet run --project AspireAppHost/AppHost.csproj
```

This will:

- Start the Aspire dashboard
- Spin up SQL Server container
- Start Azure Service Bus emulator
- Launch the Azure Functions project
- Run database migrations

### 4. Access the Dashboard

Open your browser to the Aspire dashboard URL (displayed in the console) to monitor:

- Resource health
- Logs and traces
- Metrics
- Dependencies

## 📡 API Usage

### Create a Todo

```bash
curl -X POST http://localhost:7071/api/Create \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Learn .NET Aspire",
    "isCompleted": false
  }'
```

### List Todos

```bash
curl -X POST http://localhost:7071/api/List
```

## 🧪 Running Tests

```bash
dotnet test
```

The integration tests use Aspire's testing infrastructure to:

- Spin up the entire application stack
- Verify endpoint availability
- Validate health checks

## 📦 Project Structure

```text
learning-aspire/
├── AspireAppHost/           # Orchestration layer
│   └── AppHost.cs          # Resource configuration
├── AspireServiceDefaults/   # Shared defaults
│   └── Extensions.cs       # OpenTelemetry, health checks
├── TodoFunc/               # Azure Functions project
│   ├── Migrations/         # SQL migration scripts
│   ├── Todos/
│   │   ├── Create/        # Create todo endpoint
│   │   └── List/          # List todos endpoint
│   ├── Stats/
│   │   └── OnToDoCreated.cs # Service Bus trigger
│   ├── MigrationRunner.cs
│   └── Program.cs
└── TodoFuncEndpointTests/  # Integration tests
```

## 🔧 Configuration

### Service Defaults

The `AspireServiceDefaults` project provides:

- **OpenTelemetry** - Distributed tracing and metrics
- **Health Checks** - `/health` and `/alive` endpoints
- **Service Discovery** - Automatic service resolution
- **Resilience** - Standard retry policies for HTTP clients

### Resource Configuration

Resources are configured in `AppHost.cs`:

```csharp
// SQL Server with persistent container
var sql = builder.AddSqlServer("sqlserver")
    .WithImage("mssql/server:2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

// Azure Service Bus emulator
var serviceBus = builder.AddAzureServiceBus("myservicebus")
    .RunAsEmulator(container =>
        container.WithLifetime(ContainerLifetime.Persistent));
```

## 🎯 Learning Objectives

This project demonstrates:

1. **Aspire Orchestration** - Managing multi-container applications
2. **Service Integration** - Connecting Azure Functions with databases and messaging
3. **Observability** - Implementing OpenTelemetry for monitoring
4. **Testing** - Writing integration tests for distributed systems
5. **Migrations** - Database schema management
6. **Event-Driven Architecture** - Using Service Bus for async communication

## 📚 Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [OpenTelemetry for .NET](https://opentelemetry.io/docs/languages/net/)

## 📄 License

This project is licensed under the Apache License 2.0 - see the code headers for details.

## 🤝 Contributing

This is a learning project. Feel free to fork and experiment!

---

**Note**: This project uses containerized resources. Ensure Docker Desktop is running before starting the application.
