using RobotBattle.Messages;
using RobotBattle.Models;

namespace RobotBattle.Commands
{
    internal class RobotReadyHandler : ICommand
    {
        private readonly Action<Message, Robot?> action;

        // Définition du traitement à exécuter lorsque l'action d'un joueur est reçue
        public RobotReadyHandler(Action<Message, Robot?> action)
        {
            this.action = action;
        }

        // Exécution du traitement enregistré en lui transmettant le message et le robot concerné
        public void Handle(Message message, Robot? robot)
        {
            action(message, robot);
        }
    }
}
