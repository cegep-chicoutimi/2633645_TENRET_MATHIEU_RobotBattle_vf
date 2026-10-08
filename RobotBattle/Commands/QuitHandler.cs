using RobotBattle.Messages;
using RobotBattle.Models;

namespace RobotBattle.Commands
{
    internal class QuitHandler : ICommand
    {
        private readonly Action<Message, Robot?> action;

        public QuitHandler(Action<Message, Robot?> action)
        {
            this.action = action;
        }

        public void Handle(Message message, Robot? robot)
        {
            action(message, robot);
        }
    }
}
