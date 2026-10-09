using RobotBattle.Messages;
using Serilog;
using System.Net.Sockets;
using System.Text;

namespace RobotBattle.Network
{
    internal class ConnectionHandler : IDisposable
    {
        private readonly Socket socket;
        private static readonly ILogger Logger = Log.ForContext<ConnectionHandler>();

        private const string EOM = "<|EOM|>";

        public ConnectionHandler(Socket socket)
        {
            this.socket = socket;
        }

        // Sérialization du message TCP en lui ajoutant une balise EOM afin de connaitre sa fin
        public void SendMessage(Message message)
        {
            string serializedMessage = MessageHelper.SerializeMessage(message);
            Logger.Information("Send MSG : " + serializedMessage);
            string messageWithEom = serializedMessage + EOM;

            byte[] messageBytes = Encoding.UTF8.GetBytes(messageWithEom);

            SendAll(messageBytes);
        }

        // Réception des octets jusqu'au délimiteur (balise EOM)
        public Message ReceiveMessage()
        {
            StringBuilder builder = new StringBuilder();

            while (true)
            {
                byte[] buffer = new byte[1024];

                int received = socket.Receive(buffer);

                if (received <= 0)
                {
                    throw new SocketException();
                }

                string receivedPart = Encoding.UTF8.GetString(buffer, 0, received);

                builder.Append(receivedPart);

                string receivedMessage = builder.ToString();

                int eomIndex = receivedMessage.IndexOf(EOM);

                if (eomIndex >= 0)
                {
                    string serializedMessage = receivedMessage.Substring(0, eomIndex);
                    Logger.Information("Received MSG : " + serializedMessage);
                    return MessageHelper.ParseMessage(serializedMessage);
                }
            }
        }

        // Répétition de l'envoi jusqu'à ce que l'intégralité du message ait été reçu
        private void SendAll(byte[] data)
        {
            int totalSent = 0;

            while (totalSent < data.Length)
            {
                int sent = socket.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);

                if (sent <= 0)
                {
                    throw new SocketException();
                }

                totalSent += sent;
            }
        }

        // Méthode dispose pour fermer le socket
        public void Dispose()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (SocketException)
            {
            }

            socket.Close();
            socket.Dispose();
        }
    }
}
