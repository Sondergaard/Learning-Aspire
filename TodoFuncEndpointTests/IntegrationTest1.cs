using AppHost;
using Microsoft.Extensions.Logging;

namespace TodoFuncEndpointTests;

public class IntegrationTest1
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(cancellationToken);
        
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
  
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        // The app has already waited for the health check to pass before StartAsync completes
        var httpClient = app.CreateHttpClient(AppHosts.ToDoFunction);
        await app.ResourceNotifications.WaitForResourceHealthyAsync(AppHosts.ToDoFunction, cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        
        
        var response = await httpClient.GetAsync("/api/health", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}




