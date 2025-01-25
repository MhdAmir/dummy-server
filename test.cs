// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Grpc.Net.Client;
// using Pipelines;

// partial class Program // Add the 'partial' modifier here
// {
//     static async Task Main(string[] args)
//     {
//         // Create a gRPC channel to the server
//         using var channel = GrpcChannel.ForAddress("http://localhost:50051");

//         // Create the client
//         var client = new PipelineService.PipelineServiceClient(channel);

//         // Prepare the request
//         var request = new PipelineRequest
//         {
//             Id = Guid.NewGuid().ToString(), // Generate a unique ID
//             Status = "start",
//             Streamid = { "stream1", "stream2" }, // List of stream IDs
//             Preprocessing = new Preprocessing
//             {
//                 Resize = new Resize { Width = 640, Height = 480 },
//                 Roi = new ROI
//                 {
//                     Active = false,
//                     X = 0,
//                     Y = 0,
//                     Width = 0,
//                     Height = 0
//                 }
//             },
//             Detection = new Detection
//             {
//                 Ppe = new PPE { Active = true, Threshold = 90 }
//             }
//         };

//         try
//         {
//             // Call the AddPipeline RPC
//             var response = await client.AddPipelineAsync(request);

//             // Print the response message
//             Console.WriteLine($"Response: {response.Message}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error: {ex.Message}");
//         }
//     }
// }
