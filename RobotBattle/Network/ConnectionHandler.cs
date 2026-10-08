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

        public void SendMessage(Message message)
        {
            string serializedMessage =
                MessageHelper.SerializeMessage(message);
            Logger.Information("Send MSG : " + serializedMessage);
            string messageWithEom =
                serializedMessage + EOM;

            byte[] messageBytes =
                Encoding.UTF8.GetBytes(messageWithEom);

            SendAll(messageBytes);
        }

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

                string receivedPart =
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        received
                    );

                builder.Append(receivedPart);

                string receivedMessage =
                    builder.ToString();

                int eomIndex =
                    receivedMessage.IndexOf(EOM);

                if (eomIndex >= 0)
                {
                    string serializedMessage =
                        receivedMessage.Substring(
                            0,
                            eomIndex
                        );
                    Logger.Information("Received MSG : " + serializedMessage);
                    return MessageHelper.ParseMessage(
                        serializedMessage
                    );
                }
            }
        }

        private void SendAll(byte[] data)
        {
            int totalSent = 0;

            while (totalSent < data.Length)
            {
                int sent = socket.Send(
                    data,
                    totalSent,
                    data.Length - totalSent,
                    SocketFlags.None
                );

                if (sent <= 0)
                {
                    throw new SocketException();
                }

                totalSent += sent;
            }
        }

        public void Dispose()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(
                        SocketShutdown.Both
                    );
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

//using RobotBattle.Messages;
//using System.Net.Sockets;
//using System.Text;

//namespace RobotBattle.Network
//{
//    internal class ConnectionHandler : IDisposable
//    {
//        private readonly Socket socket;

//        public ConnectionHandler(Socket socket)
//        {
//            this.socket = socket;
//        }

//        public void SendMessage(Message message)
//        {
//            string serializedMessage = MessageHelper.SerializeMessage(message);
//            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMessage);
//            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

//            SendAll(lengthBytes);
//            SendAll(messageBytes);
//        }

//        public Message ReceiveMessage()
//        {
//            byte[] lengthBytes = ReceiveAll(sizeof(int));
//            int messageLength = BitConverter.ToInt32(lengthBytes, 0);

//            if (messageLength <= 0 || messageLength > 65536)
//            {
//                throw new InvalidDataException("Taille de message invalide !");
//            }

//            byte[] messageBytes = ReceiveAll(messageLength);
//            string serializedMessage = Encoding.UTF8.GetString(messageBytes);

//            return MessageHelper.ParseMessage(serializedMessage);
//        }

//        private void SendAll(byte[] data)
//        {
//            int totalSent = 0;
//            while (totalSent < data.Length)
//            {
//                int sent = socket.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);
//                if (sent <= 0)
//                {
//                    throw new SocketException();
//                }
//                totalSent += sent;
//            }
//        }

//        private byte[] ReceiveAll(int length)
//        {
//            byte[] buffer = new byte[length];
//            int totalReceived = 0;

//            while (totalReceived < length)
//            {
//                int received = socket.Receive(buffer, totalReceived, length - totalReceived, SocketFlags.None);
//                if (received <= 0)
//                {
//                    throw new SocketException();
//                }
//                totalReceived += received;
//            }

//            return buffer;
//        }

//        public void Dispose()
//        {
//            try
//            {
//                if (socket.Connected)
//                {
//                    socket.Shutdown(SocketShutdown.Both);
//                }
//            }
//            catch (SocketException)
//            {
//            }

//            socket.Close();
//            socket.Dispose();
//        }
//    }
//}
