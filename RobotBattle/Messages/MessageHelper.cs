using RobotBattle.Models;
using System.Text.Json;

namespace RobotBattle.Messages
{
    internal static class MessageHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Construction des messages
        public static Message BuildMessage(MessageType type, GameAction? action = null, GameStatus status = GameStatus.WAITING_FOR_PLAYER, string data = "")
        {
            return new Message
            {
                Type = type,
                Action = action,
                Status = status,
                Data = data
            };
        }

        // Sérialization du message pour sa transmission sur le réseau
        public static string SerializeMessage(Message message)
        {
            return JsonSerializer.Serialize(message, JsonSerializerOption);
        }

        // Désérialisation du message pour le convertir en objet Message
        public static Message ParseMessage(string serializedMessage)
        {
            Message? parsedMessage = JsonSerializer.Deserialize<Message>(serializedMessage, JsonSerializerOption);

            if (parsedMessage == null)
            {
                throw new InvalidDataException("Message réseau invalide !");
            }

            return parsedMessage;
        }

        // Encodage des points de configurations du robot
        public static string SerializeRobotConfig(RobotConfig config)
        {
            return $"{config.HpPoints};{config.ArmorPoints};{config.DamagePoints}";
        }

        // Validation et reconstruction de la configuration à partir de l'encodage reçu
        public static RobotConfig ParseRobotConfig(string data)
        {
            string[] parts = data.Split(';');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int hp) || !int.TryParse(parts[1], out int armor) || !int.TryParse(parts[2], out int damage))
            {
                throw new InvalidDataException("Configuration de robot invalide !");
            }

            return new RobotConfig(hp, armor, damage);
        }
    }
}
