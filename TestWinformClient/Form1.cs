
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using TestWinformClient.Protos;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TestWinformClient.Protos.Connections;
using Status = Grpc.Core.Status;

namespace TestWinformClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SuccessfulUnixClients = new List<Greeter.GreeterClient>();
            SuccessfulTcpClients = new Dictionary<string, bool>();
            //IPAddressTxtBox.Text = "192.168.1.160";
            portTxtBox.Text = "5001";
        }

        private Greeter.GreeterClient? GreeterClient { get; set; }

        private List<Greeter.GreeterClient> SuccessfulUnixClients { get; set; }

        private Dictionary<string, bool> SuccessfulTcpClients { get; set; }

        private string IpAddress { get; set; }

        private bool Disconnected { get; set; }

        private bool HealthCheckRunning { get; set; }

        private async void TestConnectionBtn_Click(object sender, EventArgs e)
        {
            var port = portTxtBox.Text;
            if (string.IsNullOrEmpty(port))
            {
                debugTxtBox.Text += "Please enter a port number.\r\n";
                return;
            }

            int portNum;
            if (!Int32.TryParse(port, out portNum))
            {
                debugTxtBox.Text += "Please only enter numbers.\r\n";
                return;
            }

            Connections.ConnectionsClient? client;
            GrpcChannel? channel;
            if (UnableToFormConnection(portNum, out client, out channel))
            {
                return;
            }

            if (SuccessfulTcpClients.Count == 0)
            {
                SuccessfulTcpClients.Add(channel.ToString(), true);
            }

            if (!HealthCheckRunning)
            {
                if (SuccessfulTcpClients[channel.ToString()])
                {
                    GreeterClient = new Greeter.GreeterClient(GrpcChannel.ForAddress($"http://{IpAddress}:{portNum}"));
                    debugTxtBox.Text += "Establishing health connection to server: check background services.\r\n";
                    activePortLbl.Text = $"{IpAddress}:{portNum}";
                    await EstablishHealthConnection(client);
                    SuccessfulTcpClients[channel.ToString()] = false;
                    return;
                }

                debugTxtBox.Text += "Re-establishing connection and health check.\r\n";
                await EstablishHealthConnection(client);
                return;
            }

            if (HealthCheckRunning)
            {
                GreeterClient = new Greeter.GreeterClient(GrpcChannel.ForAddress($"http://{IpAddress}:{portNum}"));
                debugTxtBox.Text += "Already connected to server with health check running.\r\n";
            }

        }

        private bool UnableToFormConnection(int portNumber, out Connections.ConnectionsClient? client, out GrpcChannel? channel)
        {
            try
            {
                var testChannel = GrpcChannel.ForAddress($"http://{IpAddress}:{portNumber}");
                var testClient = new Connections.ConnectionsClient(testChannel);

                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;
                var reply = testClient.TestConnection(input);
                Cursor.Current = Cursors.Default;

                if (!debugTxtBox.Text.Contains($"{reply.ConnectionResponse}{portNumber}."))
                {
                    debugTxtBox.Text += $"{reply.ConnectionResponse}{portNumber}.\r\n";
                }
                client = testClient;
                channel = testChannel;
                return false;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                debugTxtBox.Text += $"The connection to port: {portNumber} was NOT SUCCESFUL.\r\n";
                client = null;
                channel = null;
                return true;
            }
        }

        private void ServerReplyButton_Click(object sender, EventArgs e)
        {
            if (GreeterClient == null)
            {
                return;
            }

            var reply = GreeterClient.SayHello(new HelloRequest { Name = nameTxtBox.Text });
            debugTxtBox.Text += $"The server responded with: {reply.Message}\r\n";
        }

        private void IPTestBtn_Click(object sender, EventArgs e)
        {
            string iPAddress = IPAddressTxtBox.Text;
            if (string.IsNullOrEmpty(iPAddress))
            {
                IpAddress = "localhost";
                debugTxtBox.Text += "IP address is set to http://localhost.\r\n";
                return;
            }

            try
            {
                Ping pingSender = new Ping();

                PingReply reply = pingSender.Send(iPAddress);
                var address = reply.Address;

                if (reply.Status == IPStatus.Success)
                {
                    debugTxtBox.Text += $"Ping from {iPAddress} was successful! Round-trip time: " +
                                        reply.RoundtripTime + "ms}.\r\n";
                    IpAddress = iPAddress;
                }

            }
            catch (Exception ex)
            {
                debugTxtBox.Text += $"Ping from {iPAddress} failed!.\r\n";
            }

        }

        private void UnixSocketBtn_Click(object sender, EventArgs e)
        {
            string SocketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");
            var channel = CreateChannel(SocketPath);
            try
            {
                var testClient = new Connections.ConnectionsClient(channel);
                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;
                var reply = testClient.TestConnection(input);

                Cursor.Current = Cursors.Default;
                var outputMessage = reply.ConnectionResponse;
                outputMessage = outputMessage.Substring(0, outputMessage.Length - 7);
                SuccessfulUnixClients.Add(new Greeter.GreeterClient(channel));
                debugTxtBox.Text += $"{outputMessage} Unix Socket: {SocketPath}.\r\n";
            }
            catch (Exception ex)
            {
                debugTxtBox.Text += $"Unable to connect to Unix Socket: {SocketPath}.\r\n";
            }

        }

        public static GrpcChannel CreateChannel(string SocketPath)
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(SocketPath);
            var connectionFactory = new UnixDomainSocketsConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler
            });
        }

        private void UnixGetServerReply_Click(object sender, EventArgs e)
        {
            if (SuccessfulUnixClients == null)
            {
                return;
            }
            var client = SuccessfulUnixClients[0];
            var reply = client.SayHello(new HelloRequest { Name = UnixNameTxtBox.Text });
            debugTxtBox.Text += $"The server responded from the Unix Socket with: {reply.Message}\r\n";
        }

        private async Task EstablishHealthConnection(Connections.ConnectionsClient client)
        {
            int call = 0;
            HealthCheckRunning = true;
            while (true)
            {
                if (DisconnectionEventFired())
                {
                    break;
                }
                try
                {
                    await SendHealthCheckUpdate(client, call++);
                }
                catch (Exception e)
                {
                    await AttemptReconnection(client);
                    break;
                }
            }

        }

        private async Task SendHealthCheckUpdate(ConnectionsClient client, int call)
        {
            var serverResponse = await client.EstablishConnectionHealthCheckAsync(new ConnectionRequest());
            backgroundTxtBox.Text += $"Server connection attempt ({call}) is: Serving.\r\n";
            backgroundTxtBox.Text += $"Currently using License: {serverResponse.Licence}\r\n";
            backgroundTxtBox.Text += $"Connected clients: {serverResponse.ConnectedClients}.\r\n";
            backgroundTxtBox.Text += "\r\n";
            ConnectionStatusLbl.Text = "Connected";
            ConnectionStatusLbl.BackColor = Color.Green;
            backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
            backgroundTxtBox.ScrollToCaret();
            await Task.Delay(5000);
        }

        private async Task AttemptReconnection(ConnectionsClient client)
        {
            for (var i = 1; i < 11; i++)
            {
                try
                {
                    if (i == 1)
                    {
                        debugTxtBox.Text += "Connection lost, attmpeting reconnection to server.\r\n";
                    }
                    backgroundTxtBox.Text += $"Reattempting connection: ({i}/10).\r\n";
                    ConnectionStatusLbl.Text = "Reconnecting...";
                    ConnectionStatusLbl.BackColor = Color.Orange;
                    backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
                    backgroundTxtBox.ScrollToCaret();

                    await Task.Delay(1000);
                    var serverResponse =
                        await client.EstablishConnectionHealthCheckAsync(new ConnectionRequest());

                    backgroundTxtBox.Text += $"Connection reestablished.\r\n";
                    backgroundTxtBox.Text += "\r\n";
                    debugTxtBox.Text += "Connection reestablished.\r\n";
                    backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
                    backgroundTxtBox.ScrollToCaret();
                    await EstablishHealthConnection(client);
                    break;
                }
                catch (Exception exception)
                {
                    if (i == 10)
                    {
                        HealthCheckRunning = false;
                        debugTxtBox.Text += $"Error connecting to server.\r\n";
                        ConnectionStatusLbl.Text = "Connection lost";
                        ConnectionStatusLbl.BackColor = Color.Red;
                        break;
                    }
                }
            }
        }

        private bool DisconnectionEventFired()
        {
            if (Disconnected)
            {
                try
                {
                    Disconnected = false;
                    HealthCheckRunning = false;
                    debugTxtBox.Text += "Succsesfully disconnected from the server.\r\n";
                    ConnectionStatusLbl.Text = "No connection";
                    ConnectionStatusLbl.BackColor = Color.White;
                    Cursor.Current = Cursors.Default;
                    return true;
                }
                catch (Exception error)
                {
                    Cursor.Current = Cursors.Default;
                    debugTxtBox.Text += "Error disconnecting.\r\n";
                    return true;
                }
            }

            return false;
        }

        private void DisconnectBtn_Click(object sender, EventArgs e)
        {
            Disconnected = true;
            Cursor.Current = Cursors.WaitCursor;
        }
    }
}