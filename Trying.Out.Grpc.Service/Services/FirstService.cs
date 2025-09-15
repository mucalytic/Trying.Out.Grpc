using System.Text;
using Grpc.Core;

namespace Trying.Out.Grpc.Service.Services;

public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> Unary(Request request, ServerCallContext context) =>
        Task.FromResult(new Response { Message = $"{request.Content} from server" });

    public override async Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        var builder = new StringBuilder();
        while (await requestStream.MoveNext())
        {
            builder.Append(requestStream.Current.Content);
        }
        return new Response { Message = builder.ToString() };
    }

    public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        var header = context.RequestHeaders.Get("purpose");
        if (header is not null)
        {
            await responseStream.WriteAsync(new Response { Message = $"Purpose is {header.Value} " });
        }
        for (var i = 0; i < 10; i++)
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            await responseStream.WriteAsync(new Response { Message = i.ToString() });
        }
    }

    public override async Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            await responseStream.WriteAsync(new  Response { Message = requestStream.Current.Content });
        }
    }
}