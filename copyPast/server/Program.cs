using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace server
{

    class MainClass
    {
        [STAThread]
        static public void Main(string[] args)
        {
            Server serv = new Server();

            Console.ReadKey();
        }

    }

    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<Client> clientList = new List<Client>();

        public Server()
        {
            try {
                this.tcpListener = new TcpListener(IPAddress.Any, 8000);
                Console.WriteLine("Server running on port 8000");
                Console.WriteLine("Local end point : {0}", tcpListener.LocalEndpoint);

                this.listenThread = new Thread(new ThreadStart(ListenForClients));
                this.listenThread.Start();
            } catch {
                Console.WriteLine("Ca marche pas :(");
            }
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            int bytesRead;
            string message;
            byte[] messageByte = new byte[51200];

            while (true) {
                TcpClient tcpClient = this.tcpListener.AcceptTcpClient();

                //Get the ID
                NetworkStream clientStream = tcpClient.GetStream();
                bytesRead = clientStream.Read(messageByte, 0, 51200);
                UnicodeEncoding encoder = new UnicodeEncoding();
                message = encoder.GetString(messageByte, 0, bytesRead);

                Console.WriteLine("Connection accepted from : {0} with ID : {1}", tcpClient.Client.RemoteEndPoint, message);
                Client client = new Client(tcpClient, this, message);
                clientList.Add(client);
            }
        }

        public void removeClient(Client client)
        {
            clientList.RemoveAt(clientList.IndexOf(client));
        }

        public void sendMessagesAtAll(string message, string identificator)
        {
            foreach (Client client in clientList) {
                if (client.ID == identificator) {
                    client.SendMessage(message);
                }
            }
        }
    }

    class Client
    {
        private TcpClient tcpClient;
        private NetworkStream clientStream;
        private Server parent;
        public string ID { get; private set; }

        public Client(TcpClient tcpClient, Server parent, string ID)
        {
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            clientThread.SetApartmentState(ApartmentState.STA);
            clientThread.Start(tcpClient);
            this.parent = parent;
            this.ID = ID;
        }

        public static bool IsConnected(Socket socket)
        {
            try {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            } catch (SocketException) { return false; }
        }

        private void HandleClientComm(object client)
        {
            this.tcpClient = (TcpClient)client;
            this.clientStream = tcpClient.GetStream();

            byte[] messageByte = new byte[51200];
            string message;
            int bytesRead;

            while (true) {
                ///Thread.Sleep(100);

                bytesRead = 0;

                try {
                    bytesRead = clientStream.Read(messageByte, 0, messageByte.Length);
                    /*
                    //blocks until a client sends a message
                    if (clientStream.DataAvailable)
                    {
                        bytesRead = clientStream.Read(messageByte, 0, 51200);
                    }*/

                } catch {
                    break;
                }
                if (bytesRead == 0) {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                message = System.Text.Encoding.Unicode.GetString(messageByte, 0, bytesRead);
                Console.WriteLine("{0} on {1} > {2}", tcpClient.Client.RemoteEndPoint, ID, message);

                parent.sendMessagesAtAll(message, ID);
            }
            disconnected();
        }

        public void SendMessage(string msg)
        {
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(msg);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        private void connected()
        {
            Console.WriteLine("{0} is now connected", tcpClient.Client.RemoteEndPoint);
        }

        private void disconnected()
        {
            Console.WriteLine("{0} is now disconnected", tcpClient.Client.RemoteEndPoint);
            parent.removeClient(this);
        }
    }
}