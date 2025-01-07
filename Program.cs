// Import required namespaces
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Worker;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/start", async (HttpContext context) =>
{
    Console.WriteLine("Sending commands to gRPC server...");

    var commands = new List<WorkerCommand>
    {
        new WorkerCommand { WorkerId = "1", Action = "start" },
    };

    var response = await ManageWorkerAsync(commands);
    await context.Response.WriteAsync(response);
});

app.MapGet("/stop", async (HttpContext context) =>
{
    Console.WriteLine("Sending commands to gRPC server...");

    var commands = new List<WorkerCommand>
    {
        new WorkerCommand { WorkerId = "1", Action = "stop" },
    };

    var response = await ManageWorkerAsync(commands);
    await context.Response.WriteAsync(response);
});

// Configure HTTP GET endpoints
app.MapGet("/restart", async (HttpContext context) =>
{
    Console.WriteLine("Sending commands to gRPC server...");

    var commands = new List<WorkerCommand>
    {
        new WorkerCommand { WorkerId = "1", Action = "restart" },
    };

    var response = await ManageWorkerAsync(commands);
    await context.Response.WriteAsync(response);
});

static async Task<string> ManageWorkerAsync(IEnumerable<WorkerCommand> commands)
{

    using var channel = GrpcChannel.ForAddress("http://localhost:50051"); // Replace with actual gRPC server address
    var client = new WorkerService.WorkerServiceClient(channel);

    using var call = client.ManageWorker();

    // Send commands in a separate task
    var sendTask = Task.Run(async () =>
    {
        foreach (var command in commands)
        {
            Console.WriteLine($"Sending command: {command.Action}");
            await call.RequestStream.WriteAsync(command);
            await Task.Delay(500); 
        }
        Console.WriteLine("Sending commands to gRPC server...");
        await call.RequestStream.CompleteAsync();
    });

    // Receive responses
    var responseMessages = new List<string>();
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Received status: {response.Status}, Message: {response.Message}");
        responseMessages.Add($"WorkerId: {response.WorkerId}, Status: {response.Status}, Message: {response.Message}");
    }

    await sendTask; // Ensure all commands are sent before finishing
    return string.Join("\n", responseMessages);
}

// Run the app
app.Run();