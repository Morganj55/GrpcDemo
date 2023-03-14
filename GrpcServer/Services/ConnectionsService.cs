using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services
{
    public class ConnectionsService : Connections.ConnectionsBase
    {
        private readonly ILogger<GreeterService> _logger;
        private int _connectedClients;
        private List<string> _clientIpAddresses; 
        private List<string> _licenses;
        private Dictionary<string, string> _addressLicensePairs;

        public ConnectionsService(ILogger<GreeterService> logger)
        {
            _logger = logger;
            _connectedClients = 0;
            _clientIpAddresses = new List<string>();
            _licenses = new List<string>
            {
                "thrhsklcjv324234",
                "ejhjjsdhfsd32543",
                "ewriweijsfdkjf453"
            };
            _addressLicensePairs = new Dictionary<string, string>();
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

        public override async Task<ConnectionResponse> EstablishConnectionHealthCheck(
            ConnectionRequest request, 
            ServerCallContext serverCallContext)
        {
            var iPAddress = serverCallContext.Peer;
            if (_connectedClients == 0 || _clientIpAddresses.Count == 0)
            {
                _clientIpAddresses.Add(iPAddress);
                _connectedClients++;
                AssignLicense(iPAddress);
            }

            if (!_clientIpAddresses.Contains(iPAddress))
            {
                _clientIpAddresses.Add(iPAddress);
                _connectedClients++;
                AssignLicense(iPAddress);
            }

            var connectionResponse = new ConnectionResponse { Licence = _addressLicensePairs[iPAddress] };
            return await Task.FromResult(connectionResponse);


        }

        private void AssignLicense(string iPAddress)
        {
            if (_licenses.Count == 0)
            {
                //There are no more licenses to give out, write it into the response stream
                return;
            }

            if (_addressLicensePairs.ContainsKey(iPAddress))
            {
                //A license has already been assigned to this IP Address 
                return;
            }

            //Add the IP Address : license pair and remove the used license from the available list 
            _addressLicensePairs.Add(iPAddress, _licenses[_licenses.Count - 1]);
            _licenses.RemoveAt(_licenses.Count -1);

        }
    }
}
