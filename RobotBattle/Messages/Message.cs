using RobotBattle.Models;

namespace RobotBattle.Messages
{
    internal class Message
    {
        public MessageType Type { get; set; }
        public GameAction? Action { get; set; }
        public GameStatus? Status { get; set; } = GameStatus.WAITING_FOR_PLAYER;
        public string Data { get; set; } = "";
    }
}
