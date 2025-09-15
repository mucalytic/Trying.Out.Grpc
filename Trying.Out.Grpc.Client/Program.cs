using System.Text;
using Grpc.Core;
using Trying.Out.Grpc.Service;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5202", new GrpcChannelOptions());
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

// Unary
var response1 = await client.UnaryAsync(new Request { Content = "Hello" });
Console.WriteLine($"Unary: {response1.Message}");

// ClientStream
using var stream1 = client.ClientStream();
for (var i = 0; i < 10; i++)
{
    await stream1.RequestStream.WriteAsync(new  Request { Content = i.ToString() });
}
await stream1.RequestStream.CompleteAsync();
var response2 = await stream1;
Console.WriteLine($"ClientStream: {response2.Message}");

// ServerStream
var builder1 = new StringBuilder();
var source1 = new CancellationTokenSource();
var metadata = new Metadata { new Metadata.Entry("purpose", "pass-butter") };
using var stream2 = client.ServerStream(new Request { Content = "Hello" }, metadata);
Console.Write("ServerStream: ");
try
{
    while (await stream2.ResponseStream.MoveNext(source1.Token))
    {
        var message = stream2.ResponseStream.Current.Message;
        if (message.Contains('7')) source1.Cancel();
        builder1.Append(stream2.ResponseStream.Current.Message);
    }
}
catch (Grpc.Core.RpcException e)
{
    Console.Write(e.Status.Detail);
    Console.Write(' ');
}
Console.WriteLine(builder1);

// BiDirectionalStream
var builder2 = new StringBuilder();
var source2 = new CancellationTokenSource();
using var stream3 = client.BiDirectionalStream();
for (var i = 0; i < 10; i++)
{
    await stream3.RequestStream.WriteAsync(new Request { Content = i.ToString() });
}
await stream3.RequestStream.CompleteAsync();
while (await stream3.ResponseStream.MoveNext(source2.Token))
{
    builder2.Append(stream3.ResponseStream.Current.Message);
}
Console.WriteLine($"BiDirectionalStream: {builder2}");
