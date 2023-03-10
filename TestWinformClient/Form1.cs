
using Grpc.Net.Client;
using TestWinformClient.Protos;

namespace TestWinformClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _succesfulClients = new List<Greeter.GreeterClient>();
        }

        private Greeter.GreeterClient? GreeterClient { get; set; }

        private List<Greeter.GreeterClient> _succesfulClients { get; set; }

        private void TestConnectionBtn_Click(object sender, EventArgs e)
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

            _succesfulClients.Add(client);
            portNumbersList.Items.Add(portNum);

        }

        private bool UnableToFormConnection(int portNumber, out Greeter.GreeterClient? client)
        {
            try
            {
                var channel = GrpcChannel.ForAddress($"http://localhost:{portNumber}");
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
            if (_succesfulClients.Count == 0)
            {
                return;
            }
            GreeterClient = _succesfulClients[portNumbersList.SelectedIndex];
            debugTxtBox.Text += $"The active port has now been set to: {portNumbersList.SelectedItem}.\r\n";
            activePortLbl.Text = $"{portNumbersList.SelectedItem}";
        }
    }
}