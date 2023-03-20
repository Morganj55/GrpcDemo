using Google.Protobuf;
using Grpc.Core;
using System.IO;

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
            DriveInfo cDrive = new DriveInfo("C");
            string cDriveFolder = cDrive.RootDirectory.FullName;
            string dataFolder = Path.Combine(cDriveFolder, "GRPCTestData");
            var newCopiedFile = Path.Combine(dataFolder, "CopiedFile.txt");
            if (File.Exists(newCopiedFile)/*@"C:\Users\johns\Documents\Training projects\Networking\TestData\CopyWordList.txt")*/);
            {
                File.Delete(newCopiedFile);
            }
            try
            {
                /*var filePath = @"C:\Users\johns\Documents\Training projects\Networking\TestData\CopyWordList.txt"*/;
                using (var outputStream = new FileStream(newCopiedFile, FileMode.Create))
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

        public override async Task GetServerFilesList(ServerFilesRequest request, IServerStreamWriter<ServerFiles> responseStream, ServerCallContext context)
        {
            DriveInfo cDrive = new DriveInfo("C");
            string cDriveFolder = cDrive.RootDirectory.FullName;
            string dataFolder = Path.Combine(cDriveFolder, "GRPCTestData");
           // var directoryPath = @"C:\Users\johns\Documents\Training projects\Networking\TestData";
            var filePaths = Directory.GetFiles(dataFolder);
            foreach (var file in filePaths)
            {
                await responseStream.WriteAsync(new ServerFiles {FilePath = file});
            }

        }

        public override async Task DownloadFileFromServer(FileDataRequest request, IServerStreamWriter<FileData> responseStream, ServerCallContext context)
        {
            string filePath = request.FileName;


            ////Read the file data and send it in chunks
            var buffer = new byte[8192];
            using (var inputStream = File.OpenRead(filePath))
            {
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await responseStream.WriteAsync(new FileData { Data = ByteString.CopyFrom(buffer, 0, bytesRead) });
                }

            }

        }

        
    }
}
