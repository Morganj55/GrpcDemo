using Grpc.Core;

namespace GrpcServer.Services
{
    public class FileStreamingService : FileStreaming.FileStreamingBase
    {
        private readonly ILogger<GreeterService> _logger;

        public FileStreamingService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task<SuccessResponse> UploadFileToServer(IAsyncStreamReader<FileData> requestStream, ServerCallContext context)
        {
            try
            {
                var filePath = @"C:\Users\johns\Documents\Training projects\Networking\TestData.txt";
                using (var outputStream = new FileStream(filePath, FileMode.Create))
                {
                    while (await requestStream.MoveNext())
                    {
                        var fileBytes = requestStream.Current;
                        await outputStream.WriteAsync(fileBytes.Data.ToByteArray());
                    }
                }

                return new SuccessResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new SuccessResponse { Success = false };
            }
        }
    }
}
