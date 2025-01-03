// Import required namespaces
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using WorkerServer;
using static WorkerServer.WorkerServer; // Ensure this matches the namespace in the generated gRPC classes

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTPS and HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5246, listenOptions =>
    {
        listenOptions.UseHttps(); // Use HTTPS
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // Ensure HTTP/2 is enabled
    });
});

// Add required services
builder.Services.AddGrpc();
var app = builder.Build();

app.UseHttpsRedirection();

// Configure HTTP GET endpoints
app.MapGet("/start", async (HttpContext context) =>
{
    var response = await SendCommandAsync("start");
    Console.WriteLine("response");
    await context.Response.WriteAsync(response);
});

app.MapGet("/stop", async (HttpContext context) =>
{
    var response = await SendCommandAsync("stop");
    await context.Response.WriteAsync(response);
});

app.MapGet("/restart", async (HttpContext context) =>
{
    var response = await SendCommandAsync("restart");
    Console.WriteLine(response);
    await context.Response.WriteAsync(response);
});

app.MapGet("/getsysteminfo", async (HttpContext context) =>
{
    var response = await GetSystemInfoAsync();
    await context.Response.WriteAsync(response);
});

static async Task<string> SendCommandAsync(string command)
{
    using var channel = GrpcChannel.ForAddress("http://localhost:50051"); // Replace with actual gRPC server address
    var client = new WorkerServerClient(channel);

    var request = new ControlCommandRequest
    {
        WorkerId = "worker-123",
        Command = command switch
        {
            "start" => ControlCommand.Types.CommandType.Start,
            "stop" => ControlCommand.Types.CommandType.Stop,
            "restart" => ControlCommand.Types.CommandType.Restart,
            _ => throw new ArgumentException("Invalid command")
        }
    };

    try
    {
        var response = await client.SendCommandAsync(request);
        return $"Command '{command}' executed successfully. Response: {response.Status}";
    }
    catch (RpcException ex)
    {
        return $"Failed to execute command '{command}'. Error: {ex.Status.Detail}";
    }
}

static async Task<string> GetSystemInfoAsync()
{
    using var channel = GrpcChannel.ForAddress("http://localhost:50051"); // Replace with actual gRPC server address
    var client = new WorkerServerClient(channel);

    var request = new SystemInfoRequest();
    try
    {
        var response = await client.GetSystemInfoAsync(request);
        return $"System Info: CPU: {response.CpuUsage}, RAM: {response.SystemRam}";
    }
    catch (RpcException ex)
    {
        return $"Failed to get system info. Error: {ex.Status.Detail}";
    }
}

// Run the app
app.Run();
