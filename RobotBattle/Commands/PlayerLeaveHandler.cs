using RobotBattle.Messages;
using RobotBattle.Models;

namespace RobotBattle.Commands
{
    internal class PlayerLeaveHandler : ICommand
    {
        private readonly Action<Message, Robot?> action;

        public PlayerLeaveHandler(Action<Message, Robot?> action)
        {
            this.action = action;
        }

        public void Handle(Message message, Robot? robot)
        {
            action(message, robot);
        }
    }
}
