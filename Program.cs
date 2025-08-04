using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherMcpServer.Tools;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    services.AddHttpClient();
    services.AddSingleton<WeatherTools>();
    services.AddMcpServer();
});

var host = builder.Build();
await host.RunAsync();