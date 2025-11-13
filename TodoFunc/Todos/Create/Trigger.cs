using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace TodoFunc.Todos.Create;

public class Trigger
{
    private readonly IConfiguration _config;
    private readonly ServiceBusSender _queue;

    public Trigger(IConfiguration config, ServiceBusClient serviceBusClient)
    {
        _config = config;
        _queue = serviceBusClient.CreateSender("ToDoCreated");
    }

    [Function("Create")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var command = await JsonSerializer.DeserializeAsync<CreateToDo>(req.Body);
        if (command == null)
            return new BadRequestObjectResult("Missing or invalid request body");

        var connectionString = _config["ConnectionStrings:ToDoDatabase"] ?? _config.GetConnectionString("ToDoDatabase");
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("INSERT INTO Todos (Title, IsCompleted) OUTPUT INSERTED.Id VALUES (@title, @isCompleted);", conn);
        cmd.Parameters.AddWithValue("@title", command.Title);
        cmd.Parameters.AddWithValue("@isCompleted", command.IsCompleted);
        var id = (int)await cmd.ExecuteScalarAsync();

        var result = new ToDoCreated
        {
            Id = id,
            Title = command.Title,
            IsCompleted = command.IsCompleted
        };

        await SendMessage(result);

        return new OkObjectResult(result);
    }

    private async Task SendMessage(ToDoCreated item)
    {
        var msg = new ServiceBusMessage(BinaryData.FromObjectAsJson(item))
        {
            Subject = nameof(ToDoCreated)
        };
        await _queue.SendMessageAsync(msg);
    }
}