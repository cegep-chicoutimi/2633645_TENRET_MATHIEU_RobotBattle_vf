using RobotBattle.Messages;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace RobotBattle.Network
{
    internal class SocketServer
    {
        private Socket? serverSocket;
        private ConnectionHandler? connection;

        public ConnectionHandler? Connection => connection;

        private static readonly ILogger Logger = Log.ForContext<SocketServer>();

        public void StartServer(int port)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(1);
            Logger.Information("Serveur démarré sur le port : " + port);
        }

        public void WaitForClient()
        {
            if (serverSocket == null)
            {
                throw new InvalidOperationException("Le serveur n'est pas démarré !");
            }
            Logger.Information("Serveur en attente d'un client");

            Socket clientSocket = serverSocket.Accept();
            connection = new ConnectionHandler(clientSocket);
            Logger.Information("Client connecté au serveur");
        }

        public void RejectSecondClient()
        {
            ConnectionHandler? currentConnection = connection;

            Task.Run(() =>
            {
                while (connection == currentConnection && serverSocket != null)
                {
                    try
                    {
                        if (!serverSocket.Poll(100000, SelectMode.SelectRead))
                        {
                            continue;
                        }

                        Socket extraSocket = serverSocket.Accept();

                        using ConnectionHandler extraConnection = new ConnectionHandler(extraSocket);

                        Message joinMessage = extraConnection.ReceiveMessage();

                        if (joinMessage.Type == MessageType.PLAYER_JOIN)
                        {
                            extraConnection.SendMessage(MessageHelper.BuildMessage(MessageType.SERVER_BUSY));
                            Logger.Information("Connexion refusée car le serveur est déjà occupé");
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }

        public void DisconnectClient()
        {
            connection?.Dispose();
            connection = null;
            Logger.Information("Client déconnecté");
        }

        public void StopServer()
        {
            DisconnectClient();
            serverSocket?.Close();
            serverSocket?.Dispose();
            serverSocket = null;
        }
    }
}
