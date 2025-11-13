// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
namespace TodoFunc.Todos.List;

public class Trigger(IConfiguration config)
{
    [Function("List")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var connectionString = config["ConnectionStrings:ToDoDatabase"] ?? config.GetConnectionString("ToDoDatabase");
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT Id, Title, IsCompleted FROM Todos;", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        var todos = new List<ToDo>();
        while (await reader.ReadAsync())
        {
            todos.Add(new ToDo
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                IsCompleted = reader.GetBoolean(2)
            });
        }
        return new OkObjectResult(todos);
    }
}