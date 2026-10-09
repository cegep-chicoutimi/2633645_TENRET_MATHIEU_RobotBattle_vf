namespace RobotBattle.Models
{
    internal class Game
    {
        private string id = string.Empty;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private GameStatus status;

        public GameStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        private int currentPlayer;

        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        private bool hasEscaped;

        public bool HasEscaped
        {
            get { return hasEscaped; }
            private set { hasEscaped = value; }
        }


        private List<Robot> robots;

        public IReadOnlyList<Robot> Robots
        {
            get { return robots; }
        }

        private Config config;

        public Config Config
        {
            get { return config; }
        }

        public Game(Config config, Robot hostRobot, Robot playerRobot)
        {
            Id = Guid.NewGuid().ToString();
            this.config = config;
            robots = new List<Robot> { hostRobot, playerRobot };
            Status = GameStatus.WAITING_FOR_PLAYER_CONFIG;
            CurrentPlayer = 0;
            HasEscaped = false;
        }

        // Début de la partie
        public void StartGame()
        {
            Status = GameStatus.PLAYING;
            CurrentPlayer = 0;
        }

        // Vérification des conditions de fin de partie
        public bool CheckGameEnded()
        {
            // Fin de partie en cas de fuite
            if (HasEscaped)
            {
                Status = GameStatus.END_GAME;
                return true;
            }

            // Fin de partie en cas de robot "mort"
            bool ended = robots.Any(r => r.GetStatValue(StatsType.HP) <= 0);
            if (ended)
            {
                Status = GameStatus.END_GAME;
            }
            return ended;
        }

        // Détermination du vainqueur de la partie (si pas de fuite)
        public Robot GetWinner()
        {
            if (HasEscaped)
            {
                throw new InvalidOperationException("La partie s'est terminée sans gagant !");
            }

            if (!CheckGameEnded())
            {
                throw new InvalidOperationException("La partie n'est pas terminée !");
            }

            return robots.OrderByDescending(r => r.GetStatValue(StatsType.HP)).First();
        }

        public Robot CurrentRobot => robots[CurrentPlayer];
        public Robot OpponentRobot => robots[CurrentPlayer == 0 ? 1 : 0];

        // Changement de joueur (tour de l'autre joueur)
        public void NextTurn()
        {
            CurrentPlayer = CurrentPlayer == 0 ? 1 : 0;
        }

        // Termine la partie en cas de fuite
        public void EscapeGame()
        {
            HasEscaped = true;
            Status = GameStatus.END_GAME;
        }
    }
}
