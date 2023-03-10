using Grpc.Core;
using GrpcServer.Protos;

namespace GrpcServer.Services
{
    public class CustomerService : Customer.CustomerBase
    {
        private ILogger<CustomerService> _logger;

        public CustomerService(ILogger<CustomerService> logger)
        {
            _logger = logger;
        }

        public override Task<CustomerModel> GetCustomerInfo (CustomerLookupModel request, ServerCallContext context) 
        {
            CustomerModel output = new CustomerModel();
            Metadata md = context.RequestHeaders;
            foreach (var data in md)
            {
                Console.WriteLine(data.Key + ": " + data.Value);
            }

            if (request.UserId == 1) 
            {
                output.FirstName = "Bob";
                output.LastName = "Bob";
            }
            else
            {
                output.FirstName = "Rob";
                output.LastName = "Rob";
            }
            return Task.FromResult(output);
        }

        public override async Task GetNewCustomers (NewCustomerRequest request, IServerStreamWriter<CustomerModel> responseStream, ServerCallContext context) 
        {
            List<CustomerModel> customers = new List<CustomerModel>()
            {
                new CustomerModel()
                {
                    FirstName = "ggfd",
                    LastName = "ghdfh",
                    EmailAdress = "fhdkjfhs",
                    Age = 41,
                    IsAlive = true
                },
                  new CustomerModel()
                {
                    FirstName = "ggfd1",
                    LastName = "ghdfh1",
                    EmailAdress = "fhdkjfhs1",
                    Age = 411,
                    IsAlive = true
                }
            };

            foreach (var cust in customers)
            {
               await responseStream.WriteAsync(cust);
            }
        }
    }
}
