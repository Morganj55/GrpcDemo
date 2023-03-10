// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcServer;


var channel = GrpcChannel.ForAddress("http://localhost:5010");
var client = new Greeter.GreeterClient(channel);

var input = new HelloRequest { Name = "Morgan12" };
var reply = client.SayHello(input);

Console.WriteLine(reply.Message);


//var customerClient = new Customer.CustomerClient(channel);

//var clientRequested = new CustomerLookupModel { UserId = 1 };

//var customer = customerClient.GetCustomerInfo(clientRequested);

//Console.WriteLine(customer.FirstName + " " + customer.LastName);

//using (var call = customerClient.GetNewCustomers(new NewCustomerRequest()))
//{
//    while (await call.ResponseStream.MoveNext())
//    {
//        var currentCustomer = call.ResponseStream.Current;
//        Console.WriteLine(currentCustomer.FirstName + " " + currentCustomer.LastName);
//    }
//}
Console.ReadLine();
