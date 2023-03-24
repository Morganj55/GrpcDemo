# gRPC and .NET 

## How Clients and Servers use gRPC 

### Overview 

For the overview of how .NET uses gRPC, please see: https://learn.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-7.0  
Both clients and server solutions will need an identical copy of the "Protos" folder which contains the ".proto" files which make up the gRPC API.

#### Summary of .proto files

.proto files consist of: 

1. Syntax: The .proto file starts with a syntax declaration that specifies the version of the Protocol Buffers language used to define the messages and services.

1. Package: The package declaration specifies the namespace that will be used for the generated code.

1. Message types: A .proto file defines the data structures (or messages) that will be used to exchange data between the client and server. Each message type is defined using the "message" keyword followed by the message name and the list of fields that make up the message.

1. Field types: The fields of a message can be of various types, such as integers, strings, booleans, or other message types.

1. Enumerations: A .proto file can also define enumerations, which are named sets of constant values.

1. Services: A service defines a set of RPC (Remote Procedure Call) methods that can be called by a client. Each method is defined using the "rpc" keyword followed by the method name, input message type, output message type, and any options.

1. Options: Options provide a way to configure aspects of the generated code, such as the namespace or the output file name.

#### Client specific gRPC implementation

For C# to be generated from the .proto files, you need to install the following gRPC Nuget packages: 

1. Grpc.Tools (allows code to be generated from .proto files)
1. Grpc.Net.Client
1. Grpc

For more information, see: https://learn.microsoft.com/en-us/aspnet/core/grpc/basics?view=aspnetcore-7.0

**To get the solution to acknowledge the .proto files, they must be added to the "Project Properties" xml file**  
To do this: 
1. Click on the project 
1. Add the path to your .proto file 
```xml
<ItemGroup>
	<Protobuf Include="Protos\greet.proto" GrpcServices="Client" />
	<Protobuf Include="Protos\Connections.proto" GrpcServices="Client" />
	<Protobuf Include="Protos\FileStreaming.proto" GrpcServices="Client" />
</ItemGroup>

```

For the client, **ensure the "GrpcServices" is equal to "client"**. 

#### Server specific gRPC implementation  

Again, for C# generated code, certain packages are required: 

1. Grpc.AspNetCore (contains Grpc.Tools)


The same process (as the client) needs to be followed to get the solution to recognise the .proto files.
In terms of changing the "Project Files", the .xml needs to reference **"Server"** instead of "Client".

```xml
<ItemGroup>
    <Protobuf Include="Protos\Connections.proto" GrpcServices="Server" />
    <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
    <Protobuf Include="Protos\FileStreaming.proto" GrpcServices="Server" />
  </ItemGroup>

  ```

  If you have copied over a .proto file to use in either client or server, **ensure you change the namespace**.

  Servers implement the functionality behind the gRPC calls, and this is done typically in a "Services" folder. You can name your service whaterver you like,
  but it has to inherit from your .proto file, which will typically be called "[.proto file name].[.proto file name]Base"  
  e.g
  
	public class ConnectionsService : Connections.ConnectionsBase

If the inherited class is not appearing, make sure you **build** your solution.
To actually route the gRPC pipeline to the service class, in the servers main program.cs, you will need to add: 

	app.MapGrpcService<ConnectionsService>();	

**You need to do this for every sevice you want to implement.**

