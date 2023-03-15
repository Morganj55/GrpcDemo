using System.Diagnostics;
using Grpc.Core;
using System.Timers;
using GrpcServer;
using Timer = System.Threading.Timer;
using System.Threading;

namespace GrpcServer.Services
{
    public class ConnectionsService : Connections.ConnectionsBase
    {
        private readonly ILogger<GreeterService> _logger;
        public static int _connectedClients;
        private List<string> _clientIpAddresses;
        private List<string> _licenses;
        private Dictionary<string, string> _addressLicensePairs;
        private Dictionary<string, int> _addressConnectionCount;

        public ConnectionsService(ILogger<GreeterService> logger)
        {
            _logger = logger;
            _connectedClients = 0;
            _clientIpAddresses = new List<string>();
            _licenses = new List<string>
            {
                "12345",
                "abcde",
                "678fghi"
            };
            _addressLicensePairs = new Dictionary<string, string>();
            _addressConnectionCount = new Dictionary<string, int>();
        }

        private bool _connected { get; set; }



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
                AssignLicense(iPAddress);
            }

            if (!_clientIpAddresses.Contains(iPAddress))
            {
                AssignLicense(iPAddress);
            }

            var connectionResponse = new ConnectionResponse
            {
                Licence = _addressLicensePairs[iPAddress],
                ConnectedClients = _connectedClients
            };

            
            var response = await Task.FromResult(connectionResponse);

            if (_addressConnectionCount[iPAddress] == 0)
            {
                Task.Run(async () => await CheckForIncomingConnections(iPAddress));
            }
            _addressConnectionCount[iPAddress]++;
            return response;
        }

        private void AssignLicense(string iPAddress)
        {
            _clientIpAddresses.Add(iPAddress);
            _connectedClients++;
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
            _licenses.RemoveAt(_licenses.Count - 1);
            _addressConnectionCount.Add(iPAddress, 0);
        }


        private async Task CheckForIncomingConnections(string iPAddress)
        {
            while (true)
            {
                var startConnectionCount = _addressConnectionCount[iPAddress];
                await Task.Delay(10000);
                if (startConnectionCount < _addressConnectionCount[iPAddress])
                {
                    continue;
                }
                else
                {
                    //remove last ip address and reclaim license
                    Debug.WriteLine($"Server has lost connection to client with IP address: {iPAddress}, reclaiming assigned license: {_addressLicensePairs[iPAddress]}");
                    _addressConnectionCount[iPAddress] = 0;
                    _connectedClients--;
                    _clientIpAddresses.Remove(iPAddress);
                    _licenses.Add(_addressLicensePairs[iPAddress]);
                    _addressLicensePairs.Remove(iPAddress);
                    break;
                }

            }
        }
    }
}
