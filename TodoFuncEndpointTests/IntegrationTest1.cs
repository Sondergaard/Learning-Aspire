using AppHost;

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
  
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        // The app has already waited for the health check to pass before StartAsync completes
        var httpClient = app.CreateHttpClient(AppHosts.ToDoFunction);
        await app.ResourceNotifications.WaitForResourceHealthyAsync(AppHosts.ToDoFunction, cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        
        
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}




