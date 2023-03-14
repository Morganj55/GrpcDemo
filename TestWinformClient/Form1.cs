
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using TestWinformClient.Protos;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Status = Grpc.Core.Status;

namespace TestWinformClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _succesfulTCPClients = new List<Greeter.GreeterClient>();
            _succesfulUnixClients = new List<Greeter.GreeterClient>();
            //IPAddressTxtBox.Text = "192.168.1.160";
            portTxtBox.Text = "5001";
        }

        private Greeter.GreeterClient? GreeterClient { get; set; }

        private List<Greeter.GreeterClient> _succesfulTCPClients { get; set; }

        private List<Greeter.GreeterClient> _succesfulUnixClients { get; set; }

        private string IPAddress { get; set; }

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

            Greeter.GreeterClient? client;
            if (UnableToFormConnection(portNum, out client))
            {
                return;
            }

            foreach (int item in portNumbersList.Items)
            {
                if (item == portNum)
                {
                    return;
                }
            }

            _succesfulTCPClients.Add(client);
            portNumbersList.Items.Add(portNum);
            await CheckConnection(portNum);
        }

        private bool UnableToFormConnection(int portNumber, out Greeter.GreeterClient? client)
        {
            try
            {
                var channel = GrpcChannel.ForAddress($"http://{IPAddress}:{portNumber}");
                var testClient = new Greeter.GreeterClient(channel);

                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;
                var reply = testClient.TestConnection(input);
                Cursor.Current = Cursors.Default;

                debugTxtBox.Text += $"{reply.ConnectionResponse}{portNumber}.\r\n";
                client = testClient;
                return false;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                debugTxtBox.Text += $"The connection to port: {portNumber} was NOT SUCCESFUL.\r\n";
                client = null;
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
            debugTxtBox.Text += $"The server responded from port {portNumbersList.SelectedItem} with: {reply.Message}\r\n";
        }

        private void SetActivePortBtn_Click(object sender, EventArgs e)
        {
            if (_succesfulTCPClients.Count == 0)
            {
                return;
            }
            GreeterClient = _succesfulTCPClients[portNumbersList.SelectedIndex];
            debugTxtBox.Text += $"The active port has now been set to: {portNumbersList.SelectedItem}.\r\n";
            activePortLbl.Text = $"{IPAddress}:{portNumbersList.SelectedItem}";
        }

        private void IPTestBtn_Click(object sender, EventArgs e)
        {
            string iPAddress = IPAddressTxtBox.Text;
            if (string.IsNullOrEmpty(iPAddress))
            {
                IPAddress = "localhost";
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
                    IPAddress = iPAddress;
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
                var testClient = new Greeter.GreeterClient(channel);
                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;
                var reply = testClient.TestConnection(input);

                Cursor.Current = Cursors.Default;
                var outputMessage = reply.ConnectionResponse;
                outputMessage = outputMessage.Substring(0, outputMessage.Length - 7);
                _succesfulUnixClients.Add(testClient);
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
            if (_succesfulUnixClients == null)
            {
                return;
            }
            var client = _succesfulUnixClients[0];
            var reply = client.SayHello(new HelloRequest { Name = UnixNameTxtBox.Text });
            debugTxtBox.Text += $"The server responded from the Unix Socket with: {reply.Message}\r\n";
        }

        private async Task CheckConnection(int portNumber)
        {
            var channel = GrpcChannel.ForAddress($"http://{IPAddress}:{portNumber}");
            var healthClient = new Health.HealthClient(channel);
            var watchTask = healthClient.Watch(new HealthCheckRequest());
            var watch = watchTask.ResponseStream;
            try
            {
                while (await watch.MoveNext())
                {
                    var response = watch.Current;
                    debugTxtBox.Text += $"Health status: {response.Status}\r\n";
                }
            }
            finally
            {
                //await watch.DisposeAsync();
            }

        }
    }
}