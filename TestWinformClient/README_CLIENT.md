 
# **GRPC Client Documentation**

## **How the Client Uses GRPC**

In terms of the client solution, there are three main elements that you need to be aware of when incorporating GRPC. 

1. GRPC packages
1. Protos folder and .proto files
1. Updating the client project properties XML file 

### **.proto files**

When creating a GRPC client solution, you need to create a "Protos" folder that will store your .proto files.
These files are essentially contracts, which are held by both client and server which allows for language agnostic communication.
Files on either sides must be identical for the solution to build, so any changes on one side must be made on the other.
  
#### .proto syntax 
  
Proto files should always include the current syntax which is:  
  
	syntax = "proto3";

  
Namespaces are needed so that C# code can reference the .proto file generated C# code. Typically, the namespace should be where your Proto folder is located.  
  
	option csharp_namespace = "TestWinformClient.Protos";

Next is to define the "Service". This is equivalent to a C# class.
  
	service Connections {
	rpc TestConnection (TestRequest) returns (TestReply);
	rpc EstablishConnectionHealthCheck (ConnectionRequest) returns (ConnectionResponse);
	rpc GetAvailableLicenseCount (LicenseCountRequest) returns (LicenseCountResponse);
	}

Inside the service, you have different "rpc" or remote procedure calls. These are the equivalent to C# methods. They require an input, and return an output.  
Inputs and outputs are called "messages", and can be defined so that they can hold different things e.g  
  
	message TestRequest {

	}

	message TestReply{
	string connectionResponse = 1;
	}

	message ConnectionResponse{
	string licence = 1;
	int32 connectedClients = 2;
	}

The numbers beside the variables are not indicative of the variable value, they are a GRPC specific "field numbers", 
which are used to uniquely identify each field in a message which is essential due to the binary nature of GRPC.  

For a full list of what GRPC messages can hold, see https://grpc.io/docs/what-is-grpc/core-concepts/.  
  

To add a .proto file to the solution, use a plain text file and change the extension to ".proto" instead of ".txt".