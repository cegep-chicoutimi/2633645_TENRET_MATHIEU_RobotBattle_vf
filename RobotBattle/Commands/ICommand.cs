using RobotBattle.Messages;
using RobotBattle.Models;

namespace RobotBattle.Commands
{
    internal interface ICommand
    {
        void Handle(Message message, Robot? robot);
    }
}
