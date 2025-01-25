using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Grpc.Net.Client;
using Pipelines;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton<HttpClient>();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Endpoint for starting a pipeline
                        endpoints.MapGet("/start/{pipelineId}", async context =>
                        {
                            var pipelineId = context.Request.RouteValues["pipelineId"] as string;
                            var response = await StartPipelineAsync(pipelineId);
                            await context.Response.WriteAsync(response);
                        });

                        // Endpoint for stopping a pipeline
                        endpoints.MapGet("/stop/{pipelineId}", async context =>
                        {
                            var pipelineId = context.Request.RouteValues["pipelineId"] as string;
                            var response = await StopPipelineAsync(pipelineId);
                            await context.Response.WriteAsync(response);
                        });
                    });
                });
            });

    private static async Task<string> StartPipelineAsync(string pipelineId)
    {
        try
        {
            // Create a gRPC channel to the server
            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            
            // Create the client
            var client = new PipelineService.PipelineServiceClient(channel);
            
            // Prepare the request
            var request = new PipelineRequest
            {
                Id = pipelineId ?? Guid.NewGuid().ToString(),
                Status = "start",
                Streamid = { "stream1", "stream2" },
                Preprocessing = new Preprocessing
                {
                    Resize = new Resize { Width = 430, Height = 300 },
                    Roi = new ROI
                    {
                        Active = false,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0
                    }
                },
                Detection = new Detection
                {
                    Ppe = new PPE { Active = true, Threshold = 90 }
                }
            };

            // Call the AddPipeline RPC
            var response = await client.AddPipelineAsync(request);
            return $"Pipeline started successfully. Response: {response.Message}";
        }
        catch (Exception ex)
        {
            return $"Error starting pipeline: {ex.Message}";
        }
    }

    private static async Task<string> StopPipelineAsync(string pipelineId)
    {
        try
        {
            // Create a gRPC channel to the server
            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            
            // Create the client
            var client = new PipelineService.PipelineServiceClient(channel);
            
            // Prepare the stop request
            var request = new PipelineRequest
            {
                Id = pipelineId,
                Status = "stop"
            };

            // Call the AddPipeline RPC with stop status
            var response = await client.AddPipelineAsync(request);
            return $"Pipeline stopped successfully. Response: {response.Message}";
        }
        catch (Exception ex)
        {
            return $"Error stopping pipeline: {ex.Message}";
        }
    }
}