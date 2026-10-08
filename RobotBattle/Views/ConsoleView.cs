using RobotBattle.Models;

namespace RobotBattle.Views
{
    internal class ConsoleView : IGameView
    {
        public string AskPlayerType()
        {
            string? choice;

            do
            {
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("             ROBOT BATTLE :");
                Console.WriteLine("--------------------------------------\n");
                Console.WriteLine("1. Créer une partie");
                Console.WriteLine("2. Rejoindre une partie\n");
                Console.Write("Saisissez votre choix (1 ou 2) : ");
                choice = Console.ReadLine();

                if (choice != "1" && choice != "2")
                {
                    Console.WriteLine("\nChoix invalide (saisissez 1 ou 2) !\n");
                }
            }
            while (choice != "1" && choice != "2");

            return choice == "1" ? "SERVER" : "CLIENT";
        }

        public string AskIpAddress()
        {
            string? ipAddress;
            do
            {
                Console.Write("Adresse IP du serveur : ");
                ipAddress = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(ipAddress));

            return ipAddress;
        }

        public int AskPort()
        {
            int port;
            bool valid;

            do
            {
                Console.Write("Port : ");
                valid = int.TryParse(Console.ReadLine(), out port) && port >= 1 && port <= 65535;
                if (!valid)
                {
                    Console.WriteLine("Erreur : le port doit être compris entre 1 et 65535 !");
                }
            }
            while (!valid);

            return port;
        }

        public RobotConfig AskPlayerConfig()
        {
            Console.WriteLine("\nRépartissez vos 10 points d'amélioration :");

            while (true)
            {
                int hp = AskNonNegativeInt("Points en HP : ");
                int armor = AskNonNegativeInt("Points en armure : ");
                int damage = AskNonNegativeInt("Points en dégâts : ");

                RobotConfig config = new RobotConfig(hp, armor, damage);
                if (config.IsValid(10))
                {
                    return config;
                }

                Console.WriteLine("Configuration invalide (la somme doit être exactement égale à 10) !\n");
            }
        }

        public GameAction AskPlayerAction()
        {
            while (true)
            {
                Console.WriteLine("\n1. Attaque");
                Console.WriteLine("2. Défense");
                Console.WriteLine("3. Attaque puissante (coût : 2 énergie)");
                Console.WriteLine("4. Recharge (+1 énergie)");
                Console.WriteLine("5. Réparation (coût : 1 énergie)");
                Console.WriteLine("6. Esquive");
                Console.WriteLine("7. Fuite");
                Console.Write("Action : ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": return GameAction.ATTACK;
                    case "2": return GameAction.DEFENSE;
                    case "3": return GameAction.ATTACK_PUISSANCE;
                    case "4": return GameAction.RECHARGE;
                    case "5": return GameAction.REPAIR;
                    case "6": return GameAction.DODGE;
                    case "7": return GameAction.ESCAPE;
                    default: Console.WriteLine("Action invalide !"); break;
                }
            }
        }

        public bool AskPlayerReplay()
        {
            while (true)
            {
                Console.Write("\nVoulez-vous rejouer ? (o/n) : ");
                string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer == "o" || answer == "oui") return true;
                if (answer == "n" || answer == "non") return false;
                Console.WriteLine("Réponse invalide !");
            }
        }

        public void ShowPlayerRole(bool isHost)
        {
            Console.WriteLine("\n--------------------------------------");
            Console.WriteLine($"                {(isHost ? "HÔTE" : "CLIENT")}");
            Console.WriteLine("--------------------------------------\n");
        }

        public void ShowWinner(Robot robot)
        {
            ShowMessage(robot.IsHost ? "L'hôte gagne la partie !" : "Le client gagne la partie !");
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static int AskNonNegativeInt(string prompt)
        {
            int value;
            do
            {
                Console.Write(prompt);
            }
            while (!int.TryParse(Console.ReadLine(), out value) || value < 0);

            return value;
        }
    }
}
