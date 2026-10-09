using RobotBattle.Messages;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace RobotBattle.Network
{
    internal class SocketClient
    {
        private Socket? socket;
        private ConnectionHandler? connection;

        public ConnectionHandler? Connection => connection;

        private static readonly ILogger Logger = Log.ForContext<SocketClient>();

        // Établissement d'une connexion TCP avec l'adresse et le port de l'hôte
        public void ConnectToServer(string ipAddress, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            socket.Connect(serverEndPoint);
            connection = new ConnectionHandler(socket);
            Logger.Information("Connection du client au serveur réussie : " + ipAddress + ":" + port);
        }

        public bool IsConnected()
        {
            return socket != null && socket.Connected;
        }

        // Fermeture de la connexion
        public void Exit()
        {
            connection?.Dispose();
            connection = null;
            socket = null;
        }
    }
}
