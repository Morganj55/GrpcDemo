using GrpcServer.Services;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using static System.Console;


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService()
        ? AppContext.BaseDirectory
        : default
});


// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenLocalhost(5000);
    options.ListenLocalhost(5010);
    options.ListenAnyIP(5001);
    options.ListenAnyIP(5002);
    options.ListenAnyIP(5003);
    options.ListenAnyIP(5006); // listen on port 5000
});

// Add services to the container.
builder.Host.UseWindowsService();
builder.Services.AddGrpcReflection();
//builder.WebHost.UseKestrel();
builder.Services.AddGrpc();
// Add reflection services
//builder.Services.AddGrpc


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<CustomerService>();
//app.UseHttpsRedirection();
//app.MapControllers();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

WriteLine("Server has started and is listening on port: 1");
app.Run();



