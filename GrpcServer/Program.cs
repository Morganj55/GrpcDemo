using System.Net;
using System.Net.Sockets;
using System.Text;
using GrpcServer.Services;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using static System.Console;


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService()
        ? AppContext.BaseDirectory
        : default
});
static string GenerateRandomString(int length)
{
    const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(Chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}

string folderPath = $"{Path.GetTempPath()}GRPCUnixSocket";
if (!Directory.Exists(folderPath))
{
    // Create the folder
    Directory.CreateDirectory(folderPath);
    Console.WriteLine("Folder created successfully.");
}
else
{
    Console.WriteLine("Folder already exists.");
}

string filePath = Path.Combine(folderPath, "config.txt");
string socketPath;
using (StreamWriter streamWriter = File.CreateText(filePath))
{
    socketPath = Path.Combine(folderPath, GenerateRandomString(5) + ".tmp");
    streamWriter.Write(socketPath);
    streamWriter.Close();
}

foreach (var file in Directory.EnumerateFiles(folderPath))
{
    if (Path.GetExtension(file) == ".tmp")
    {
        File.Delete(file);
    }
}


DriveInfo cDrive = new DriveInfo("C");
string cDriveFolder = cDrive.RootDirectory.FullName;
string dataFolder = Path.Combine(cDriveFolder,"GRPCTestData");
if (!Directory.Exists(dataFolder))
{
    // Create the folder
    Directory.CreateDirectory(dataFolder);
    Console.WriteLine("Folder created successfully.");
}
else
{
    Console.WriteLine("Folder already exists.");
}

foreach (var file in Directory.EnumerateFiles(dataFolder))
{
    if (Path.GetExtension(file) == ".txt")
    {
        File.Delete(file);
    }
}

async void StartServer(int portNumber)
{
    UdpClient server = new UdpClient(portNumber);
    server.EnableBroadcast = true;

    while (true)
    {
        UdpReceiveResult receiveResult = await server.ReceiveAsync();

        byte[] discoveryMessage = receiveResult.Buffer;

        // Handle the discovery message and send a response

        byte[] serverInfo = Encoding.ASCII.GetBytes("SERVER_INFO");
        await server.SendAsync(serverInfo, serverInfo.Length, receiveResult.RemoteEndPoint);
    }
}


int portNumber = 8101;
Task.Run(() => StartServer(portNumber));


if (File.Exists(socketPath))
{
    File.Delete(socketPath);
}
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenLocalhost(5000);
    options.ListenAnyIP(5001);
    options.ListenUnixSocket(socketPath);
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Add services to the container.
builder.Host.UseWindowsService();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();
builder.Services.AddSingleton<ConnectionsService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ConnectionsService>();
app.MapGrpcService<FileStreamingService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
WriteLine("Server has started and is listening:");
app.Run();


//const int portNumber = 8101;
//Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, portNumber);
//serverSocket.Bind(localEndpoint);

//while (true)
//{
//    byte[] buffer = new byte[1024];
//    EndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
//    int bytesReceived = serverSocket.ReceiveFrom(buffer, ref clientEndpoint);
//    string discoveryMessage = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

//    // Handle the discovery message and send a response

//    byte[] serverInfo = Encoding.ASCII.GetBytes("SERVER_INFO");
//    serverSocket.SendTo(serverInfo, clientEndpoint);
//}

