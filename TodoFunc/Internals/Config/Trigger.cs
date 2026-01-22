using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TodoFunc.Internals.Config;

public class Trigger(IConfiguration configuration)
{
    [Function("ListConfig")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var value = configuration["MyKey"] ?? "Key not found";
        
        return new OkObjectResult(value);
        
    }

}