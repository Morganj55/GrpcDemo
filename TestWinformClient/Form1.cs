
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
using Microsoft.AspNetCore.Hosting;
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

        private List<EndPoint> DiscoveredServerIpv4Addresses { get; set; }

        private List<EndPoint> DiscoveredServerIpv6Addresses { get; set; }

        /// <summary>
        /// Attempts to establish a gRPC connection to a server using specified IP address and port number 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            string? channel;
            if (UnableToFormConnection(portNum, out client, out channel))
            {
                return;
            }

            // The first time a channel comes through, its added to a dictionary, the value of true indicates its brand new and hasn't undergone health checks
            // Setting the value to false indicates this particular channel has already undergone health checks, which will change the output in the debug textbox
            if (SuccessfulTcpClients.Count == 0)
            {
                SuccessfulTcpClients.Add(channel, true);
            }

            // Checks for an already running health check occuring 
            if (!HealthCheckRunning)
            {
                // if the channel is a brand new channel and hasnt already passed through a health check (its dictionary value will be true),
                // the initial health check will be established 
                if (SuccessfulTcpClients[channel])
                {
                    GreeterClient = new Greeter.GreeterClient(GrpcChannel.ForAddress($"http://{IpAddress}:{portNum}"));
                    debugTxtBox.Text += "Establishing health connection to server: check background services.\r\n";
                    activeAddressLbl.Text = $"{IpAddress}:{portNum}";

                    // The health check is an infinite loop that can only be broken through using the "Disconnect" button, or the server crashing 
                    // Upon disconnection, the channels dictionariy value is set to false, indicating that this particular channel has already been used,
                    // which will change the output in the debug text box upon reconnecting
                    await EstablishHealthConnection(client);
                    SuccessfulTcpClients[channel] = false;
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

        /// <summary>
        /// Attempts to form a gRPC connection using a port number given, and using the IP address previously set in "IPTestBtn"
        /// If successful, produces a working client and a channel reference used in the calling method 
        /// </summary>
        /// <param name="portNumber"></param>
        /// <param name="client"></param>
        /// <param name="channel"></param>
        /// <returns>Bool if the connection was successful</returns>
        private bool UnableToFormConnection(int portNumber, out Connections.ConnectionsClient? client, out string? channel)
        {
            try
            {
                var testChannel = GrpcChannel.ForAddress($"http://{IpAddress}:{portNumber}");
                var testClient = new Connections.ConnectionsClient(testChannel);

                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;

                //At this point, if there is no reply, an exception will be thrown 
                var reply = testClient.TestConnection(input);
                Cursor.Current = Cursors.Default;

                if (!debugTxtBox.Text.Contains($"{reply.ConnectionResponse}{portNumber}."))
                {
                    debugTxtBox.Text += $"{reply.ConnectionResponse}{portNumber}.\r\n";
                }
                // Sets the current client used for the file streaming service 
                CurrentFileStreamClient = new FileStreaming.FileStreamingClient(testChannel);
                client = testClient;
                channel = testChannel.ToString();
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

        /// <summary>
        /// Part of the initial template, its an example of using gRPC calls to contact a server and get a response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerReplyButton_Click(object sender, EventArgs e)
        {
            if (GreeterClient == null)
            {
                return;
            }

            var reply = GreeterClient.SayHello(new HelloRequest { Name = nameTxtBox.Text });
            debugTxtBox.Text += $"The server responded with: {reply.Message}\r\n";
        }

        /// <summary>
        /// Attempts to ping a user defined IP address
        /// If successful, the property IpAddress is set, which is used in the "TestConnectionBtn_Click" button event.
        /// Otherwise, an exception is caught and displayed in the debug textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IPTestBtn_Click(object sender, EventArgs e)
        {
            // Gets IP string from textbox 
            string iPAddress = IPAddressTxtBox.Text;

            // Check if the textbox is empty, if so, will set the IP as "LocalHost"
            if (string.IsNullOrEmpty(iPAddress))
            {
                IpAddress = "localhost";
                debugTxtBox.Text += "IP address is set to http://localhost.\r\n";
                return;
            }

            // sends ping to user entered IP and responds accordingly 
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

        /// <summary>
        /// Reads a socket file path from a config file and attempts to establish a gRPC connection 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UnixSocketBtn_Click(object sender, EventArgs e)
        {
            // Reads the socket path generated by the server
            var socketPath = ReadUnixSocketFromConfigFile();

            try
            {
                var channel = CreateChannel(socketPath);
                var testClient = new Connections.ConnectionsClient(channel);
                var input = new TestRequest { };
                Cursor.Current = Cursors.WaitCursor;

                // An exception will be thrown here if a connection cant be established 
                var reply = testClient.TestConnection(input);

                Cursor.Current = Cursors.Default;
                var outputMessage = reply.ConnectionResponse;
                outputMessage = outputMessage.Substring(0, outputMessage.Length - 7);

                // Sets current client so that other services can use this active client 
                SuccessfulUnixClients.Add(new Greeter.GreeterClient(channel));
                CurrentFileStreamClient = new FileStreaming.FileStreamingClient(channel);

                debugTxtBox.Text += $"{outputMessage} Unix Socket: {socketPath}.\r\n";
                activeAddressLbl.Text = socketPath;
                await EstablishHealthConnection(testClient);
            }
            catch (Exception ex)
            {
                debugTxtBox.Text += $"Unable to connect to Unix Socket.\r\n";
            }

        }

        /// <summary>
        /// Read UDP socket file path generated by the server
        /// </summary>
        /// <returns>Unix socket file path string</returns>
        private static string? ReadUnixSocketFromConfigFile()
        {
            // When a server starts up, it will generate a folder called "GRPCUnixSocket" in the temp data area, where it will create a
            // text file called "config.txt" which will contain the specific socket path string that the client can use
            string filePath = Path.GetTempPath() + Path.Combine("GRPCUnixSocket", "config.txt");
            string socketPath = null;
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
                        socketPath = line;
                    }

                    // Close the file
                    sr.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("File not found.");
                socketPath = null;
            }

            return socketPath;
        }

        /// <summary>
        /// Create a unix socket channel that gRPC can use 
        /// </summary>
        /// <param name="SocketPath"></param>
        /// For more detail on where the code came from, see: https://learn.microsoft.com/en-us/aspnet/core/grpc/interprocess-uds?view=aspnetcore-7.0
        /// <returns>Unix socket specific gRPC channel</returns>
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

        /// <summary>
        /// Uses the greeter service to show a gRPC call working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// This is the heartbeat methods that establishes a constant connection between server and client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task EstablishHealthConnection(Connections.ConnectionsClient client)
        {
            // call is set so that each time SendHealthCheckUpdate is called, the method can write into the "Background Services" text box the number of calls
            int call = 0;
            HealthCheckRunning = true;

            // Enters into an infinite loop unless the disconnection button is pressed, or the server disappears and attempt reconnection fails
            while (true)
            {
                if (DisconnectionEventFired())
                {
                    break;
                }
                try
                {
                    // Method that sends the health connection request, method is delayed for 5 seconds and then infinite loop continues
                    await SendHealthCheckUpdate(client, call++);
                }
                catch (Exception e)
                {
                    // if an exception is thrown in SendHealthCheckUpdate, it will be caught and reconnection will be tried
                    // if reconnection is successful, "AttemptReconnection" actually recalls "EstablishHealthConnection" and restarts this infinite loop
                    // if an exception is thrown after 10 reconnection attempts, it will return out of "AttemptReconnection" and then break "EstablishHealthConnection"s infinite loop 
                    await AttemptReconnection(client);
                    break;
                }
            }

        }

        /// <summary>
        /// Method that sends health check connection requests through gRPC
        /// </summary>
        /// <param name="client"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        private async Task SendHealthCheckUpdate(ConnectionsClient client, int call)
        {
            // send the connection request 
            var serverResponse = await client.EstablishConnectionHealthCheckAsync(new ConnectionRequest());

            // if successful, various labels on the form will be set with the results return from the health check 
            backgroundTxtBox.Text += $"Server connection attempt ({call}) is: Serving.\r\n";
            backgroundTxtBox.Text += $"Connected clients: {serverResponse.ConnectedClients}.\r\n";
            backgroundTxtBox.Text += "\r\n";
            backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
            backgroundTxtBox.ScrollToCaret();

            // This includes the unique license that the server has 
            activeLicenseLbl.Text = $"{serverResponse.Licence}";

            connectionStatusLbl.Text = "Connected";
            connectionStatusLbl.BackColor = Color.Green;

            // task is delayed every 5 seconds so the calling infinite loop is delayed
            await Task.Delay(5000);
        }

        /// <summary>
        /// If the server crashes or is turned off, this method provides some leniency for reconnecting if it comes back online
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task AttemptReconnection(ConnectionsClient client)
        {
            // Reconnection will be attempted 10 times 
            for (var i = 1; i < 11; i++)
            {
                try
                {
                    // The first attempt will trigger the first debug text 
                    if (i == 1)
                    {
                        debugTxtBox.Text += "Connection lost, attmpeting reconnection to server.\r\n";
                    }

                    // Background window text will state the reconnetion attmpet number 
                    backgroundTxtBox.Text += $"Reattempting connection: ({i}/10).\r\n";
                    backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
                    backgroundTxtBox.ScrollToCaret();

                    //Form will show that the connection was lost and that its attempting to reconnect 
                    connectionStatusLbl.Text = "Reconnecting...";
                    connectionStatusLbl.BackColor = Color.Orange;

                    // The reconnection attempt will be delayed by x seconds, as to allow time for the server to come back online
                    await Task.Delay(2000);

                    // if it can connect, an exception will be thrown here 
                    var serverResponse =
                        await client.EstablishConnectionHealthCheckAsync(new ConnectionRequest());

                    // Form will show that it has reconnected to the server 
                    backgroundTxtBox.Text += $"Connection reestablished.\r\n";
                    backgroundTxtBox.Text += "\r\n";
                    backgroundTxtBox.SelectionStart = backgroundTxtBox.Text.Length;
                    backgroundTxtBox.ScrollToCaret();
                    debugTxtBox.Text += "Connection reestablished.\r\n";

                    // Reconnection will then call the orignial health connection method and will restart the infinite loop 
                    // The only way to break out of these loops will be through the disconnection event firing or if reconnection cannot occur after 10 attempts
                    await EstablishHealthConnection(client);
                    break;
                }
                catch (Exception exception)
                {
                    if (i == 10)
                    {
                        HealthCheckRunning = false;
                        debugTxtBox.Text += $"Error connecting to server.\r\n";
                        connectionStatusLbl.Text = "Connection lost";
                        connectionStatusLbl.BackColor = Color.Red;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check for if the Disconnected flag has been set to true
        /// Sets various form labels and text to indicate the result of the flags result
        /// This bool is checked every x seconds in the calling methods infinite loop
        /// </summary>
        /// <returns></returns>
        private bool DisconnectionEventFired()
        {
            if (Disconnected)
            {
                try
                {
                    // sets flags that will stop the health check from running and will break it from its infinite loop
                    Disconnected = false;
                    HealthCheckRunning = false;

                    // sets various labels and text indicating the state of the form 
                    activeAddressLbl.Text = "";
                    debugTxtBox.Text += "Succsesfully disconnected from the server.\r\n";
                    connectionStatusLbl.Text = "No connection";
                    connectionStatusLbl.BackColor = Color.White;
                    activeLicenseLbl.Text = "";
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

        /// <summary>
        /// Button that sets the disconnected flag
        /// Will break the health checks that have already been established
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectBtn_Click(object sender, EventArgs e)
        {
            Disconnected = true;
            Cursor.Current = Cursors.WaitCursor;
            debugTxtBox.Text += "Attempting to disconnect, please wait...\r\n";
        }

        /// <summary>
        /// Used to set the filepath for the file you want to upload 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                // Sets the file you want to upload that is used in the "UploadFileBtn_click" method
                FileUploadPath = filePath;
            }
        }

        /// <summary>
        /// Gets a client stream for uploading the file, and then reads the file data in chunks and sends it to the server.
        /// After all the data has been sent, the method signals the end of the stream and waits for the server to respond.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UploadFileBtn_Click(object sender, EventArgs e)
        {
            // Get a client stream for uploading the file
            var stream = CurrentFileStreamClient.UploadFileToServer();

            // Display a message indicating that the file upload has started
            debugTxtBox.Text += "File upload started...\r\n";

            // Read the file data and send it in chunks
            var buffer = new byte[8192];
            using (var inputStream = File.OpenRead(FileUploadPath))
            {
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    // Write the file data to the client stream
                    await stream.RequestStream.WriteAsync(new FileData { Data = ByteString.CopyFrom(buffer, 0, bytesRead) });
                }

            }

            // Signal the end of the stream
            await stream.RequestStream.CompleteAsync();
            Cursor.Current = Cursors.Default;

            // Wait for the server to respond
            var response = await stream.ResponseAsync;

            // Display a message indicating whether the file upload was successful
            if (response.Success)
            {
                debugTxtBox.Text += "File successfully uploaded.\r\n";
            }
            else
            {
                debugTxtBox.Text += "File upload unsuccesfull.\r\n";
            }
        }

        /// <summary>
        /// Gets a stream for retrieving the list of files from the server, initializes a list of file paths and a combo box for displaying the available files,
        /// and loops through the files in the response stream and adds them to the list and combo box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetFilesBtnClick(object sender, EventArgs e)
        {
            // Display a message indicating that the files are being retrieved from the server
            debugTxtBox.Text += "Getting files from server, please wait.\r\n";

            try
            {
                // Get a stream for retrieving the list of files from the server
                using var stream = CurrentFileStreamClient.GetServerFilesList(new ServerFilesRequest());

                // Initialize a list of file paths and a combo box for displaying the available files
                AvailableServerFilesToDownload = new List<string>();
                DownloadFilesComboBox.Items.Clear();

                // Loop through the files in the response stream and add them to the list and combo box
                while (await stream.ResponseStream.MoveNext())
                {
                    AvailableServerFilesToDownload.Add(stream.ResponseStream.Current.FilePath);

                }

                // Adds the available download files to the combo box so that the user can choose
                foreach (var file in AvailableServerFilesToDownload)
                {
                    DownloadFilesComboBox.Items.Add(Path.GetFileName(file));
                }

                // Display a message indicating that the files are available for download
                debugTxtBox.Text += "Files are available to download.\r\n";
            }
            catch (Exception ex)
            {
                // Display a message indicating that an error occurred while retrieving the files
                debugTxtBox.Text += "Error retrieving files.\r\n";
            }
        }

        /// <summary>
        /// Downloads the specific file from the server to the disk. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DownloadFileBtn_Click(object sender, EventArgs e)
        {
            // Check if there are files available to download
            if (DownloadFilesComboBox.Items.Count == 0)
            {
                return;
            }

            // Get the file path of the selected file to download from the server
            var serverDownloadFilePath = AvailableServerFilesToDownload[DownloadFilesComboBox.SelectedIndex];

            // Create a SaveFileDialog to prompt the user to select a location to save the downloaded file
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Configure the SaveFileDialog settings
            saveFileDialog.InitialDirectory = @"C:\Users\johns\Documents\Training projects\Networking\TestClientDownload";
            saveFileDialog.FileName = "myFile.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.CheckFileExists = false; // Ensure file does not already exist
            saveFileDialog.CheckPathExists = false; // Ensure path is valid

            // Show the SaveFileDialog and wait for the user to select a file
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected folder path from the SaveFileDialog
                string folderPath = Path.GetDirectoryName(saveFileDialog.FileName);

                try
                {
                    // Set the cursor to a wait cursor while downloading
                    Cursor.Current = Cursors.WaitCursor;

                    // Start the file download from the server
                    debugTxtBox.Text += "File download started...\r\n";
                    using var stream = CurrentFileStreamClient.DownloadFileFromServer(
                        new FileDataRequest { FileName = serverDownloadFilePath }
                        );

                    // Write the downloaded file to disk
                    using (var outputStream = new FileStream(Path.Combine(folderPath, saveFileDialog.FileName), FileMode.Create))
                    {
                        // Waits for incoming data packets and writes them to the disk as soon as they're received
                        while (await stream.ResponseStream.MoveNext())
                        {
                            // Read the file data from the server stream and write it to the output stream
                            Cursor.Current = Cursors.WaitCursor;
                            var fileBytes = stream.ResponseStream.Current;
                            await outputStream.WriteAsync(fileBytes.Data.ToByteArray());
                        }
                    }

                    // Update the UI to indicate that the file has been successfully downloaded
                    debugTxtBox.Text += "File successfully downloaded.\r\n";
                    Cursor.Current = Cursors.Default;

                }
                catch (Exception ex)
                {
                    // Update the UI to indicate that an error occurred while downloading the file
                    debugTxtBox.Text += "Error downloading file.\r\n";
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Discovers server IPV4 addresses and writes them to the debug textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DiscoverServersIpv4Btn_Click(object sender, EventArgs e)
        {
            // The port number that the servers are listening to
            // This port number has been chosen at random and is arbitrary
            const int portNumber = 8101;

            // Set the cursor to wait and show a message in the debug text box
            Cursor.Current = Cursors.WaitCursor;
            debugTxtBox.Text += $"Searching for available servers, please wait...\r\n";

            // Initialize a list to store the discovered server IPv4 addresses and call the async function to discover the servers
            DiscoveredServerIpv4Addresses = new List<EndPoint>();
            DiscoveredServerIpv4Addresses = await DiscoverServersIpv4Async(portNumber);

            // Display the number of discovered servers in the debug text box
            debugTxtBox.Text += $" ({DiscoveredServerIpv4Addresses.Count}) servers discovered.\r\n";

            // Loop through each discovered IPv4 address, remove the port number and display the IP address in the debug text box
            foreach (var ipAddress in DiscoveredServerIpv4Addresses)
            {
                var iPString = RemovePortNumberFromIpv4(ipAddress);
                debugTxtBox.Text += $"Server IP address:{iPString}\r\n";
            }
        }

        /// <summary>
        /// Discovers IPv4 servers on the network through UDP discovery.
        /// creates a UDP socket, sends a broadcast packet to all network interfaces on the subnet, and listens for incoming responses.
        /// </summary>
        /// <param name="portNumber"></param>
        /// <returns>It returns a list of endpoints for all servers that responded to the broadcast.</returns>
        private static async Task<List<EndPoint>> DiscoverServersIpv4Async(int portNumber)
        {
            // Create an empty list to store server responses
            List<EndPoint> serverResponses = new List<EndPoint>();

            // Create a UDP socket for broadcasting
            using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // Set socket options to allow broadcasting
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                // Create a discovery packet to send as a broadcast
                byte[] discoveryPacket = Encoding.ASCII.GetBytes("DISCOVER");

                // Set the broadcast address and port number
                // The reason this broadcast address is chosen is because this is a "multicast address".
                // Its the same one that WS-Discovery uses and its not blocked by AntiVirus programs 
                // A multicast is different from a broadcast address, as this address will broadcast just to local networks
                // and only servers that are subscribed to this address will be able to receive and respond to the UDP packets 
                var broadcastAddress = (IPAddress.Parse("239.255.255.250"));
                IPEndPoint broadcastEndpoint = new IPEndPoint(broadcastAddress, portNumber);

                // Send the discovery packet as a broadcast
                await clientSocket.SendToAsync(new ArraySegment<byte>(discoveryPacket), SocketFlags.None, broadcastEndpoint);

                // Create a buffer to store incoming responses
                byte[] responseBuffer = new byte[1024];

                // Set the server endpoint to listen on all network interfaces
                // IPAddress.Any allows the endpoint to listen on all network interfaces that the machine has available, so that it can receive traffic from any source.
                // Passing 0 into the port number actually means the the operating system will assign an available port number to the endpoint automatically, and will choose one not in use.
                EndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);

                // Start receiving responses in the background
                var receiveTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            // Receive an incoming response and store the sender's endpoint
                            SocketReceiveFromResult receiveResult =
                                await clientSocket.ReceiveFromAsync(new ArraySegment<byte>(responseBuffer),
                                    SocketFlags.None, serverEndpoint);

                            // Convert the response data to a string
                            string serverResponse =
                                Encoding.ASCII.GetString(responseBuffer, 0, receiveResult.ReceivedBytes);

                            // Add the sender's endpoint to the list of server responses
                            serverResponses.Add(receiveResult.RemoteEndPoint);
                        }
                        catch (Exception ex)
                        {
                            // Exit the loop if there is an exception
                            break;
                        }

                    }
                });

                // Wait for a specified amount of time before closing the socket and stopping the background receive task
                // When the socket is closed, a socket exception will be thrown inside the Task which will break out of the infinite loop 
                await Task.Delay(10000);
                clientSocket.Close();
                await receiveTask;
            }
            // Return the list of server responses
            return serverResponses;
        }

        /// <summary>
        /// Requests the number of available licenses from each discovered server IPV4 address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetDiscoveredLicensesIpv4_Click(object sender, EventArgs e)
        {
            // Reset available network licenses each time 
            int availableNetworkLicenses = 0;

            // This is the port that the Kestrel server is configured to listen on 
            int portNumber = 5001;

            // If no servers are found, display a message and return.
            if (DiscoveredServerIpv4Addresses.Count == 0)
            {
                debugTxtBox.Text += $"No servers found.\r\n";
                return;
            }

            // Iterate through each discovered server IP address.
            foreach (var iPAddress in DiscoveredServerIpv4Addresses)
            {
                try
                {
                    // Clear active address label
                    activeAddressLbl.Text = "";

                    // Get the IP address string without the port number.
                    var iPString = RemovePortNumberFromIpv4(iPAddress);

                    // Create a gRPC channel for the current IP address and port number.
                    var testChannel = GrpcChannel.ForAddress($"http://{iPString}:{portNumber}");
                    var testClient = new Connections.ConnectionsClient(testChannel);

                    // Create a new request to get the available license count from the server.
                    var input = new LicenseCountRequest { };

                    // Set the cursor to the wait cursor while waiting for the server response.
                    Cursor.Current = Cursors.WaitCursor;

                    // Send the request to the server and wait for the response.
                    var reply = await testClient.GetAvailableLicenseCountAsync(input);
                    Cursor.Current = Cursors.Default;

                    // Display the server's IP address and the number of available licenses.
                    debugTxtBox.Text += $"Server IP:{iPString} has:({reply.NumberOfAvailableLicenses}) license(s).\r\n";

                    // Add the number of available licenses to the total number of available network licenses.
                    availableNetworkLicenses += reply.NumberOfAvailableLicenses;
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;

                    // Display a message indicating that the connection to the server was unsuccessful.
                    debugTxtBox.Text += $"The connection to {RemovePortNumberFromIpv4(iPAddress)}:{portNumber} was NOT SUCCESFUL.\r\n";
                }
            }

            // Display the total number of available network licenses.
            debugTxtBox.Text += $"Available Network Licenses: ({availableNetworkLicenses}).\r\n";
        }

        /// <summary>
        /// Removes the port number from IPV4 addresses
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>IPV4 address without the ":{portNumber}" substring</returns>
        private string RemovePortNumberFromIpv4(EndPoint ipAddress)
        {
            // Converts the EndPoint to a string 
            var iPString = ipAddress.ToString();

            // Searches the string for the ':' index and takes the substring up to that index
            for (int i = 0; i < iPString.Length; i++)
            {
                if (iPString[i] == ':')
                {
                    return iPString.Substring(0, i);
                }
            }
            return null;
        }

        /// <summary>
        /// From the discovered IPV4 addresses, connects to the first one with an available license 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DiscoveryConnectIpv4Btn_Click(object sender, EventArgs e)
        {
            // Reset available network licenses each time 
            int availableNetworkLicenses = 0;

            // This is the port that the Kestrel server is configured to listen on 
            int portNumber = 5001;

            // Check if any servers were found
            if (DiscoveredServerIpv4Addresses.Count == 0)
            {
                debugTxtBox.Text += $"No servers found.\r\n";
                return;
            }

            // Loop through each server IP address found
            foreach (var iPAddress in DiscoveredServerIpv4Addresses)
            {
                try
                {
                    // Clear active address label
                    activeAddressLbl.Text = "";

                    // Get the IP address string and create a gRPC channel with the server
                    var iPString = RemovePortNumberFromIpv4(iPAddress);
                    var testChannel = GrpcChannel.ForAddress($"http://{iPString}:{portNumber}");

                    // Create a new ConnectionsClient object using the channel
                    var testClient = new Connections.ConnectionsClient(testChannel);

                    // Create a new LicenseCountRequest object and retrieve the number of available licenses from the server
                    var input = new LicenseCountRequest { };
                    Cursor.Current = Cursors.WaitCursor;
                    var reply = await testClient.GetAvailableLicenseCountAsync(input);
                    Cursor.Current = Cursors.Default;
                    availableNetworkLicenses += reply.NumberOfAvailableLicenses;

                    // If there are available licenses, establish a health connection to a server with an available license and set the active address label
                    if (reply.NumberOfAvailableLicenses > 0)
                    {
                        debugTxtBox.Text += "Establishing health connection to server: check background services.\r\n";
                        activeAddressLbl.Text = $"{iPString}";
                        await EstablishHealthConnection(testClient);
                    }
                }
                catch (Exception ex)
                {
                    // If the connection was not successful, set the cursor to default and display an error message in the debug text box
                    Cursor.Current = Cursors.Default;
                    debugTxtBox.Text += $"The connection to {RemovePortNumberFromIpv4(iPAddress)}:{portNumber} was NOT SUCCESFUL.\r\n";
                }
            }

            // Display the total available network licenses in the debug text box
            debugTxtBox.Text += $"Available Network Licenses: ({availableNetworkLicenses}).\r\n";
        }

        /// <summary>
        /// Discovers server IPV6 addresses and writes them to the debug textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DiscoverServersIPV6Btn_Click(object sender, EventArgs e)
        {
            // This is the port number that the Kestrel server is configured to listen to IPV6 UDP requests (arbitrary port number)
            const int portNumber = 9101;

            // It sets the cursor to wait mode and updates the debug text box to indicate that it's searching for servers
            Cursor.Current = Cursors.WaitCursor;
            debugTxtBox.Text += $"Searching for available servers, please wait...\r\n";

            // Calls the DiscoverServersIpv6Async method to asynchronously discover servers and stores the results in the DiscoveredServerIpv6Addresses list
            DiscoveredServerIpv6Addresses = new List<EndPoint>();
            DiscoveredServerIpv6Addresses = await DiscoverServersIpv6Async(portNumber);

            // Updates the debug text box with the number of servers discovered
            debugTxtBox.Text += $" ({DiscoveredServerIpv6Addresses.Count}) servers discovered.\r\n";

            // Iterates through the DiscoveredServerIpv6Addresses list and gets the IPv6 address string of each server,
            // and updates the debug text box with the IPv6 address of each discovered server.
            foreach (var ipAddress in DiscoveredServerIpv6Addresses)
            {
                var iPString = RemovePortNumberFromIpv6(ipAddress);
                debugTxtBox.Text += $"Server IP address:{iPString}\r\n";
            }
        }

        /// <summary>
        /// Discovers server IPV6 addresses using UDP broadcast 
        /// </summary>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        private static async Task<List<EndPoint>> DiscoverServersIpv6Async(int portNumber)
        {
            // Initializing the server responses list
            List<EndPoint> serverResponses = new List<EndPoint>();

            // Multicast address for IPV6 is arbitrary
            // This multicast address is a "Link-local scope" IPV6 multicast address and can go up to "ff02::/16"
            IPAddress multicastAddress = IPAddress.Parse("ff02::1");

            // Creating a new IPV6 socket client
            using (var clientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp))
            {
                // Adding membership to multicast group
                clientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(multicastAddress));

                // Binding the socket to receive data
                // 8102 is an arbitrary port number
                clientSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 8102));

                // Sending a "DISCOVER" packet to the multicast endpoint
                byte[] discoveryPacket = Encoding.ASCII.GetBytes("DISCOVER");
                IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, portNumber);
                await clientSocket.SendToAsync(new ArraySegment<byte>(discoveryPacket), SocketFlags.None, multicastEndpoint);

                // Initializing the response buffer
                byte[] responseBuffer = new byte[1024];

                // Creating an endpoint to listen for incoming traffic from any source on any available network interface
                EndPoint serverEndpoint = new IPEndPoint(IPAddress.IPv6Any, 0);

                // Starting a task to receive incoming traffic and add the source endpoint to the serverResponses list
                var receiveTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            // Receiving data from any endpoint
                            SocketReceiveFromResult receiveResult =
                                await clientSocket.ReceiveFromAsync(new ArraySegment<byte>(responseBuffer),
                                    SocketFlags.None, serverEndpoint);

                            // Converting received data to string format
                            string serverResponse =
                                Encoding.ASCII.GetString(responseBuffer, 0, receiveResult.ReceivedBytes);

                            // Adding the source endpoint to the serverResponses list
                            serverResponses.Add(receiveResult.RemoteEndPoint);
                        }
                        catch (Exception ex)
                        {
                            // Breaking the loop if there is an exception
                            break;
                        }

                    }
                });

                // Waiting for 5 seconds for incoming traffic, then closing the socket
                // When the socket is closed, a socket exception will be thrown inside the Task which will break out of the infinite loop 
                await Task.Delay(5000);
                clientSocket.Close();
                await receiveTask;
            }
            // Returning the list of discovered server endpoints
            return serverResponses;
        }

        /// <summary>
        /// Requests the number of available licenses from each discovered server IPV4 address 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetDiscoveredLicensesIpv6_Click(object sender, EventArgs e)
        {
            // Reset available network licenses each time 
            int availableNetworkLicenses = 0;

            // This is the port that the Kestrel server is configured to listen on 
            int portNumber = 5001;

            // Check if any servers have been discovered. If not, display a message and return.
            if (DiscoveredServerIpv6Addresses.Count == 0)
            {
                debugTxtBox.Text += $"No servers found.\r\n";
                return;
            }

            // Loop through each discovered server and attempt to connect to it
            foreach (var iPAddress in DiscoveredServerIpv6Addresses)
            {
                try
                {
                    // Set the activeAddressLbl to empty and get the IP address string
                    activeAddressLbl.Text = "";
                    var iPString = RemovePortNumberFromIpv6(iPAddress);

                    // Create a new gRPC channel and client
                    var testChannel = GrpcChannel.ForAddress($"http://{iPString}:{portNumber}");
                    var testClient = new Connections.ConnectionsClient(testChannel);

                    // Send a request to get the available license count
                    var input = new LicenseCountRequest { };
                    Cursor.Current = Cursors.WaitCursor;
                    var reply = await testClient.GetAvailableLicenseCountAsync(input);
                    Cursor.Current = Cursors.Default;

                    // Display the IP address and available license count for the server
                    debugTxtBox.Text += $"Server IP:{iPString} has:({reply.NumberOfAvailableLicenses}) license(s).\r\n";
                    availableNetworkLicenses += reply.NumberOfAvailableLicenses;
                }
                catch (Exception ex)
                {
                    // Display a message if the connection was unsuccessful
                    Cursor.Current = Cursors.Default;
                    debugTxtBox.Text += $"The connection to {RemovePortNumberFromIpv6(iPAddress)}:{portNumber} was NOT SUCCESFUL.\r\n";
                }
            }

            debugTxtBox.Text += $"Available Network Licenses: ({availableNetworkLicenses}).\r\n";
        }

        /// <summary>
        /// Removes the port number from a IPV6 address
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private string RemovePortNumberFromIpv6(EndPoint ipAddress)
        {
            var iPString = ipAddress.ToString();
            for (int i = iPString.Length - 1; i > 0; i--)
            {
                if (iPString[i] == ':')
                {
                    return iPString.Substring(0, i);
                }
            }
            return null;
        }

        /// <summary>
        /// From the discovered IPV6 addresses, connects to the first one with an available license 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DiscoveryConnectIpv6Btn_Click(object sender, EventArgs e)
        {

            // Reset available network licenses each time 
            int availableNetworkLicenses = 0;

            // This is the port that the Kestrel server is configured to listen on 
            int portNumber = 5001;

            // Check if any servers were found
            if (DiscoveredServerIpv6Addresses.Count == 0)
            {
                debugTxtBox.Text += $"No servers found.\r\n";
                return;
            }

            foreach (var iPAddress in DiscoveredServerIpv6Addresses)
            {
                try
                {
                    // Clear active address label
                    activeAddressLbl.Text = "";

                    // Create a new ConnectionsClient object using the channel
                    var iPString = RemovePortNumberFromIpv6(iPAddress);
                    var testChannel = GrpcChannel.ForAddress($"http://{iPString}:{portNumber}");
                    var testClient = new Connections.ConnectionsClient(testChannel);

                    var input = new LicenseCountRequest { };
                    Cursor.Current = Cursors.WaitCursor;
                    var reply = await testClient.GetAvailableLicenseCountAsync(input);
                    Cursor.Current = Cursors.Default;
                    availableNetworkLicenses += reply.NumberOfAvailableLicenses;

                    // If there are available licenses, establish a health connection to a server with an available license and set the active address label
                    if (reply.NumberOfAvailableLicenses > 0)
                    {
                        debugTxtBox.Text += "Establishing health connection to server: check background services.\r\n";
                        activeAddressLbl.Text = $"{iPString}";
                        await EstablishHealthConnection(testClient);
                    }
                }
                catch (Exception ex)
                {
                    // If the connection was not successful, set the cursor to default and display an error message in the debug text box
                    Cursor.Current = Cursors.Default;
                    debugTxtBox.Text += $"The connection to {RemovePortNumberFromIpv4(iPAddress)}:{portNumber} was NOT SUCCESFUL.\r\n";
                }
            }

            // Display the total available network licenses in the debug text box
            debugTxtBox.Text += $"Available Network Licenses: ({availableNetworkLicenses}).\r\n";
        }
    }
}
