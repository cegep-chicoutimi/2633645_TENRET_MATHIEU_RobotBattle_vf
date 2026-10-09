using RobotBattle.Messages;
using RobotBattle.Models;
using System.Windows.Input;

namespace RobotBattle.Commands
{
    internal class CommandMenu
    {
        private readonly Dictionary<MessageType, ICommand> handlers = new();

        // Enregistrement du handler au type de message associé
        public void AddHandler(MessageType type, ICommand handler)
        {
            handlers[type] = handler;
        }

        // Traitement du message à partir de son handler
        public void Execute(Message message, Robot? from)
        {
            if (!handlers.TryGetValue(message.Type, out ICommand? handler))
            {
                throw new InvalidOperationException($"Aucun handler pour {message.Type} !");
            }

            handler.Handle(message, from);
        }
    }
}
