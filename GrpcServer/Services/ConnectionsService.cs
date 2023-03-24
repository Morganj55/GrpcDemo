using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

            //var iPForToken = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            //    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
           // var token = $"({iPForToken}) : {GenerateRandomString(5)}";
           var token = GenerateRandomString(5);

            _licenses = new List<string>
            {
                token
            };
            _addressLicensePairs = new Dictionary<string, string>();
            _addressConnectionCount = new Dictionary<string, int>();
        }

       private string GenerateRandomString(int length)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(Chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
            //assign the IP address from the meta data
            var iPAddress = serverCallContext.Peer;
            

            //Assigns IP Addresses to incoming client,
            bool licenseWasAssigned = AssignLicense(iPAddress);

            ConnectionResponse connectionResponse;
            if (licenseWasAssigned)
            {
                //Creates the new outgoing Connection response variable used to send back data in the GRPC call.
                connectionResponse = new ConnectionResponse
                {
                    Licence = _addressLicensePairs[iPAddress],
                    ConnectedClients = _connectedClients
                };

                //Create the response that will be sent back to the client 
                var response = await Task.FromResult(connectionResponse);

                //Starts the checks for continual health requests from the client, will only start checks once per IP address
                if (_addressConnectionCount[iPAddress] == 0)
                {
                    Task.Run(async () => await CheckForIncomingConnections(iPAddress));
                }
                _addressConnectionCount[iPAddress]++;
                return response;

            }
            else
            {
                connectionResponse = new ConnectionResponse
                {
                    Licence = "No more licenses available. You do not have permission to use the software.",
                    ConnectedClients = _connectedClients
                };
                return await Task.FromResult(connectionResponse);
            }

        }

        private bool AssignLicense(string iPAddress)
        {
            if (_connectedClients == 0 || _clientIpAddresses.Count == 0)
            {
                //First new client is always assigned a new license
                UpdateLicenseInfo(iPAddress);
                return true;
            }
            if (_addressLicensePairs.ContainsKey(iPAddress))
            {
                //A license has already been assigned to this IP Address 
                return true;
            }

            if (!_clientIpAddresses.Contains(iPAddress) && _licenses.Count != 0)
            {
                //New clients will be assigned licenses if they are available 
                UpdateLicenseInfo(iPAddress);
                return true;
            }


            //There are no more licenses to give out, write it into the response stream
            Debug.WriteLine("No more licenses available.");
            return false;

        }

        private void UpdateLicenseInfo(string iPAddress)
        {
            _clientIpAddresses.Add(iPAddress);
            _connectedClients++;
            _addressLicensePairs.Add(iPAddress, _licenses[_licenses.Count - 1]);
            _licenses.RemoveAt(_licenses.Count - 1);
            _addressConnectionCount.Add(iPAddress, 0);
        }

        private async Task CheckForIncomingConnections(string iPAddress)
        {
            while (true)
            {
                //preDelayConnection count is set to the IP address specific connection count
                //during Task.Delay, the IP address specific connection count will be incremented (every 5 seconds) if the client is still connected 
                //if it hasnt been incremented, both numbers will be the same which means the client has disconnected
                var preDelayConnectionCount = _addressConnectionCount[iPAddress];
                await Task.Delay(10000);
                if (preDelayConnectionCount < _addressConnectionCount[iPAddress])
                {
                    continue;
                }
                else
                {
                    //remove last ip address and reclaim license
                    Debug.WriteLine($"Server has lost connection to client with IP address: {iPAddress}, reclaiming assigned license: {_addressLicensePairs[iPAddress]}");
                    Console.WriteLine($"Server has lost connection to client with IP address: {iPAddress}, reclaiming assigned license: {_addressLicensePairs[iPAddress]}");
                    //_addressConnectionCount[iPAddress] = 0;
                    _addressConnectionCount.Remove(iPAddress);
                    _clientIpAddresses.Remove(iPAddress);
                    _licenses.Add(_addressLicensePairs[iPAddress]);
                    _addressLicensePairs.Remove(iPAddress);
                    _connectedClients--;
                    break;
                }

            }
        }

        public override Task<LicenseCountResponse> GetAvailableLicenseCount(LicenseCountRequest request, ServerCallContext context)
        {
            return Task.FromResult(new LicenseCountResponse
            {
                NumberOfAvailableLicenses = _licenses.Count
            });
        }
    }
}
