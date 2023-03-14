using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<TestReply> TestConnection(TestRequest request, ServerCallContext context)
        {
            // Delete the socket file if it already exists
            var socketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");
            if (File.Exists(socketPath))
            {
                File.Delete(socketPath);
            }

            return Task.FromResult(new TestReply
            {
                ConnectionResponse = "The server successfully established a connection with port: "
            });
        }

    }
}