
using System.Collections.Generic;
using System.Dynamic;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Windows.Forms;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.VisualBasic.ApplicationServices;
using TestWinformClient.Protos;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TestWinformClient.Protos.Connections;
using File = System.IO.File;
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

        private FileStreaming.FileStreamingClient CurrentFileStreamClient { get; set; }

        private string IpAddress { get; set; }

        private bool Disconnected { get; set; }

        private bool HealthCheckRunning { get; set; }

        private string FileUploadPath { get; set; }

        private List<string> AvailableServerFilesToDownload { get; set; }

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
                CurrentFileStreamClient = new FileStreaming.FileStreamingClient(channel);
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

        private async void UnixSocketBtn_Click(object sender, EventArgs e)
        {
            var SocketPath = ReadUnixSocketFromConfigFile();

            try
            {
                var channel = CreateChannel(SocketPath);
                var testClient = new Connections.ConnectionsClient(channel);
                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;
                var reply = testClient.TestConnection(input);

                Cursor.Current = Cursors.Default;
                var outputMessage = reply.ConnectionResponse;
                outputMessage = outputMessage.Substring(0, outputMessage.Length - 7);

                SuccessfulUnixClients.Add(new Greeter.GreeterClient(channel));
                CurrentFileStreamClient = new FileStreaming.FileStreamingClient(channel);

                debugTxtBox.Text += $"{outputMessage} Unix Socket: {SocketPath}.\r\n";
                activePortLbl.Text = SocketPath;
                await EstablishHealthConnection(testClient);
            }
            catch (Exception ex)
            {
                debugTxtBox.Text += $"Unable to connect to Unix Socket.\r\n";
            }

        }

        private static string? ReadUnixSocketFromConfigFile()
        {
            string filePath = Path.GetTempPath() + Path.Combine("GRPCUnixSocket", "config.txt");
            string SocketPath = null;
            try
            {
                if (File.Exists(Path.GetTempPath() + Path.Combine("GRPCUnixSocket", "config.txt")))
                {
                    // Open the file for reading
                    StreamReader sr = new StreamReader(filePath);

                    // Read the file line by line
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        SocketPath = line;
                    }

                    // Close the file
                    sr.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("File not found.");
                SocketPath = null;
            }

            return SocketPath;
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
                    activePortLbl.Text = "";
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
            debugTxtBox.Text += "Attempting to disconnect, please wait...\r\n";

        }

        private void OpenFileBtn_Click(object sender, EventArgs e)
        {
            // Create a new instance of the OpenFileDialog class
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the initial directory
            openFileDialog.InitialDirectory = @"C:\Users\johns\Documents\Training projects\Networking\TestData";

            // Set the filter for the file types to display
            openFileDialog.Filter = "Text Files|*.txt|All Files|*.*";

            // Show the dialog and wait for the user to choose a file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;
                ChosenFilePathLbl.Text = filePath;
                FileUploadPath = filePath;
            }
        }

        private async void UploadFileBtn_Click(object sender, EventArgs e)
        {
            var stream = CurrentFileStreamClient.UploadFileToServer();

            ////Read the file data and send it in chunks
            debugTxtBox.Text += "File upload started...\r\n";
            var buffer = new byte[8192];
            using (var inputStream = File.OpenRead(FileUploadPath))
            {
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    await stream.RequestStream.WriteAsync(new FileData { Data = ByteString.CopyFrom(buffer, 0, bytesRead) });
                }

            }

            // Signal the end of the stream
            await stream.RequestStream.CompleteAsync();
            Cursor.Current = Cursors.Default;

            // Wait for the server to respond
            var response = await stream.ResponseAsync;


            if (response.Success)
            {
                debugTxtBox.Text += "File successfully uploaded.\r\n";
            }
            else
            {
                debugTxtBox.Text += "File upload unsuccesfull.\r\n";
            }
        }

        private async void GetFilesBtnClick(object sender, EventArgs e)
        {
            debugTxtBox.Text += "Getting files from server, please wait.\r\n";

            try
            {
                using var stream = CurrentFileStreamClient.GetServerFilesList(new ServerFilesRequest());
                AvailableServerFilesToDownload = new List<string>();
                DownloadFilesComboBox.Items.Clear();

                while (await stream.ResponseStream.MoveNext())
                {
                    AvailableServerFilesToDownload.Add(stream.ResponseStream.Current.FilePath);

                }

                foreach (var file in AvailableServerFilesToDownload)
                {
                    DownloadFilesComboBox.Items.Add(Path.GetFileName(file));
                }
                debugTxtBox.Text += "Files are available to download.\r\n";
            }
            catch (Exception ex)
            {
                debugTxtBox.Text += "Error retrieving files.\r\n";
            }


        }

        private async void DownloadFileBtn_Click(object sender, EventArgs e)
        {
            if (DownloadFilesComboBox.Items.Count == 0)
            {
                return;
            }
            var serverDownloadFilePath = AvailableServerFilesToDownload[DownloadFilesComboBox.SelectedIndex];
            // create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // set the initial directory and file name
            saveFileDialog.InitialDirectory = @"C:\Users\johns\Documents\Training projects\Networking\TestClientDownload";
            saveFileDialog.FileName = "myFile.txt";
            saveFileDialog.DefaultExt = ".txt";

            // configure the dialog to only allow the user to select a folder
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = false;

            // display the dialog and wait for the user to select a folder
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // get the selected folder path
                string folderPath = Path.GetDirectoryName(saveFileDialog.FileName);

                // download the file into the selected folder path
                // ...
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    debugTxtBox.Text += "File download started...\r\n";
                    using var stream = CurrentFileStreamClient.DownloadFileFromServer(
                        new FileDataRequest { FileName = serverDownloadFilePath }
                        );

                    using (var outputStream = new FileStream(Path.Combine(folderPath, saveFileDialog.FileName), FileMode.Create))
                    {
                        while (await stream.ResponseStream.MoveNext())
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            var fileBytes = stream.ResponseStream.Current;
                            await outputStream.WriteAsync(fileBytes.Data.ToByteArray());

                        }
                    }

                    debugTxtBox.Text += "File successfully downloaded.\r\n";
                    Cursor.Current = Cursors.Default;

                }
                catch (Exception ex)
                {
                    debugTxtBox.Text += "Error downloading file.\r\n";
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private async void DiscoverServersBtn_Click(object sender, EventArgs e)
        {
            const int portNumber = 8101;
            Cursor.Current = Cursors.WaitCursor;
            var serverResponses = await DiscoverServersAsync(portNumber);

            //Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            ////clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.AcceptConnection, 1);

            //byte[] discoveryPacket = Encoding.ASCII.GetBytes("DISCOVER");
            //IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, portNumber);
            //clientSocket.SendTo(discoveryPacket, broadcastEndpoint);

            //byte[] responseBuffer = new byte[1024];
            //EndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);

            //List<string> serverResponses = new List<string>();
            //int bytesRead = clientSocket.ReceiveFrom(responseBuffer, ref serverEndpoint);
            //string serverResponse = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
            //serverResponses.Add(serverResponse);
            
            debugTxtBox.Text += $"There are {serverResponses.Count} servers running.\r\n";

            //foreach (var ipAddress in serverResponses.Values)
            //{
            //    debugTxtBox.Text += $"Found IP addresses running the server are:{ipAddress.ToString()}\r\n";
            //}
            foreach (var ipAddress in serverResponses)
            {
                debugTxtBox.Text += $"Found IP addresses running the server are:{ipAddress.ToString()}\r\n";
            }
            Cursor.Current = Cursors.Default;
        }


        private static async Task<List<EndPoint>> DiscoverServersAsync(int portNumber)
        {
            //Dictionary<string, EndPoint>
            //Dictionary<string, EndPoint> serverResponses = new Dictionary<string, EndPoint>();
            List<EndPoint> serverResponses = new List<EndPoint>();
            //List<string> serverResponses = new List<string>();

            using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBroadcast, 1);

                byte[] discoveryPacket = Encoding.ASCII.GetBytes("DISCOVER");
                IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, portNumber);
                await clientSocket.SendToAsync(new ArraySegment<byte>(discoveryPacket), SocketFlags.None, broadcastEndpoint);

                byte[] responseBuffer = new byte[1024];
                EndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);

                //DateTime startTime = DateTime.UtcNow;

                //while ((DateTime.UtcNow - startTime).TotalSeconds < 10) // Wait for 10 seconds or until a certain number of responses have been received
                //{
                //    try
                //    {
                //        var result = await clientSocket.ReceiveFromAsync(new ArraySegment<byte>(responseBuffer), SocketFlags.None, serverEndpoint);

                //        string serverResponse = Encoding.ASCII.GetString(responseBuffer, 0, result.ReceivedBytes);
                //        serverResponses.Add(serverResponse);
                //    }
                //    catch (SocketException ex)
                //    {
                //        if (ex.SocketErrorCode == SocketError.TimedOut)
                //        {
                //            break;
                //        }
                //        else
                //        {
                //            throw;
                //        }
                //    }
                //}
                var receiveTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            SocketReceiveFromResult receiveResult =
                                await clientSocket.ReceiveFromAsync(new ArraySegment<byte>(responseBuffer),
                                    SocketFlags.None, serverEndpoint);

                            string serverResponse =
                                Encoding.ASCII.GetString(responseBuffer, 0, receiveResult.ReceivedBytes);

                            
                            serverResponses.Add(receiveResult.RemoteEndPoint);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                      
                    }
                });

                await Task.Delay(10000);
                clientSocket.Close();
                await receiveTask;
            }
            return serverResponses;
        }









    }
}
