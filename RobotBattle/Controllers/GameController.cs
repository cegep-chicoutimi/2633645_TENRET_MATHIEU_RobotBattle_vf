using RobotBattle.Commands;
using RobotBattle.Messages;
using RobotBattle.Models;
using RobotBattle.Network;
using RobotBattle.Views;
using Serilog;
using Serilog.Core;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;

namespace RobotBattle.Controllers
{
    internal class GameController
    {
        private readonly IGameView view;
        private readonly Config config;
        private readonly CommandMenu commandMenu;

        private SocketServer? socketServer;
        private SocketClient? socketClient;
        private Game? game;
        private bool isHost;

        private static readonly ILogger Logger = Log.ForContext<GameController>();

        public GameController(IGameView view)
        {
            this.view = view;
            config = new Config();
            commandMenu = new CommandMenu();
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            commandMenu.AddHandler(MessageType.PLAYER_JOIN, new PlayerJoinHandler((message, robot) => view.ShowMessage("Demande de connexion reçue !")));

            commandMenu.AddHandler(MessageType.ROBOT_CONFIG_OK, new RobotReadyHandler((message, robot) => view.ShowMessage("Configuration acceptée par le serveur !")));

            commandMenu.AddHandler(MessageType.PLAYER_ACTION, new PlayerActionHandler((message, robot) => view.ShowMessage($"Action reçue : {message.Action}")));

            commandMenu.AddHandler(MessageType.PLAYER_RESULT, new PlayerResultHandler((message, robot) => ShowResult(message)));

            commandMenu.AddHandler(MessageType.PLAYER_REPLAY, new PlayerReplayHandler((message, robot) => view.ShowMessage("Nouvelle partie demandée !")));

            commandMenu.AddHandler(MessageType.QUIT, new QuitHandler((message, robot) => view.ShowMessage("Fin de connexion demandée !")));

            commandMenu.AddHandler(MessageType.ERROR, new PlayerLeaveHandler((message, robot) => view.ShowMessage($"Erreur serveur : {message.Data}")));
        }

        public void CreateGame()
        {
            isHost = true;
            int port = view.AskPort();
            config.Port = port;
            socketServer = new SocketServer();

            try
            {
                if (isHost)
                {
                    string ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)).ToString();
                    Console.WriteLine($"\n-----------------------------------");
                    Console.WriteLine($"IP de l'hôte : {ipAddress}");
                    Console.WriteLine($"Port de l'hôte : {config.Port}");
                    Console.WriteLine($"-----------------------------------");
                }

                socketServer.StartServer(port);
                view.ShowMessage($"\nServeur démarré sur le port {port}");

                while (true)
                {
                    view.ShowMessage("En attente d'un joueur...");
                    socketServer.WaitForClient();
                    socketServer.RejectSecondClient();
                    view.ShowMessage("\nUn joueur vient de se connecter !");

                    try
                    {
                        HandleClientSession(socketServer.Connection!);
                    }
                    catch (SocketException)
                    {
                        view.ShowMessage("Le client a perdu la connexion !");
                    }
                    catch (IOException exception)
                    {
                        view.ShowMessage($"Message réseau invalide : {exception.Message}");
                    }
                    finally
                    {
                        socketServer.DisconnectClient();
                        game = null;
                    }
                }
            }
            catch (SocketException exception)
            {
                view.ShowMessage($"Erreur réseau : {exception.Message}");
            }
            finally
            {
                socketServer.StopServer();
            }
        }

        private void HandleClientSession(ConnectionHandler connection)
        {
            Message joinMessage = connection.ReceiveMessage();
            if (joinMessage.Type != MessageType.PLAYER_JOIN)
            {;
                SendError(connection, "EXPECTED_PLAYER_JOIN");
                return;
            }

            Logger.Information("Demande de connexion du joueur acceptée par le serveur");
            commandMenu.Execute(joinMessage, null);
            connection.SendMessage(MessageHelper.BuildMessage(MessageType.WELCOME, status: GameStatus.WAITING_FOR_HOST_CONFIG, data: "HOTE"));

            bool replay = true;

            while (replay)
            {
                RobotConfig hostConfig = AskValidHostConfig();
                Robot hostRobot = new Robot(true, hostConfig.ToStats(config), config);

                RobotConfig clientConfig = ReceiveValidClientConfig(connection);
                Robot clientRobot = new Robot(false, clientConfig.ToStats(config), config);

                game = new Game(config, hostRobot, clientRobot);
                game.StartGame();
                Logger.Information("Partie démarrée avec l'id : " + game.Id);

                Message startMessage = BuildStartMessage();
                ShowStart(startMessage);

                connection.SendMessage(startMessage);
                connection.SendMessage(BuildTurnMessage());

                RunServerCombat(connection);

                Message replayOrQuit = connection.ReceiveMessage();
                if (replayOrQuit.Type == MessageType.PLAYER_REPLAY)
                {
                    commandMenu.Execute(replayOrQuit, clientRobot);
                    connection.SendMessage(MessageHelper.BuildMessage(MessageType.PLAYER_REPLAY, status: GameStatus.WAITING_FOR_PLAYER_CONFIG, data: "OK"));
                    replay = true;
                }
                else if (replayOrQuit.Type == MessageType.QUIT)
                {
                    commandMenu.Execute(replayOrQuit, clientRobot);
                    connection.SendMessage(MessageHelper.BuildMessage(MessageType.QUIT, status: GameStatus.END_GAME, data: "OK"));
                    replay = false;
                }
                else
                {
                    SendError(connection, "EXPECTED_REPLAY_OR_QUIT");
                    replay = false;
                }
            }
        }

        private RobotConfig AskValidHostConfig()
        {
            RobotConfig hostConfig;
            do
            {
                hostConfig = view.AskPlayerConfig();
                if (!hostConfig.IsValid(config.PointsToGive))
                {
                    view.ShowMessage("Configuration invalide du robot !");
                }
            }
            while (!hostConfig.IsValid(config.PointsToGive));

            return hostConfig;
        }

        private RobotConfig ReceiveValidClientConfig(ConnectionHandler connection)
        {
            while (true)
            {
                Message configMessage = connection.ReceiveMessage();
                if (configMessage.Type != MessageType.ROBOT_CONFIG)
                {
                    SendError(connection, "EXPECTED_ROBOT_CONFIG");
                    continue;
                }

                try
                {
                    RobotConfig clientConfig = MessageHelper.ParseRobotConfig(configMessage.Data);
                    if (!clientConfig.IsValid(config.PointsToGive))
                    {
                        SendError(connection, "INVALID_ROBOT_CONFIG");
                        continue;
                    }

                    connection.SendMessage(MessageHelper.BuildMessage(MessageType.ROBOT_CONFIG_OK, status: GameStatus.PLAYING, data: "OK"));

                    return clientConfig;
                }
                catch (InvalidDataException)
                {
                    SendError(connection, "INVALID_ROBOT_CONFIG");
                }
            }
        }

        private void RunServerCombat(ConnectionHandler connection)
        {
            if (game == null) return;

            while (!game.CheckGameEnded())
            {
                if (game.CurrentRobot.IsHost)
                {
                    view.ShowMessage("\n--- Votre tour ---");
                    GameAction action = AskValidAction(game.CurrentRobot);
                    Message result = ExecuteAction(action);
                    ShowResult(result);
                    connection.SendMessage(result);
                }
                else
                {
                    view.ShowMessage("\n--- Tour du client ---");
                    Message actionMessage = connection.ReceiveMessage();

                    if (actionMessage.Type != MessageType.PLAYER_ACTION)
                    {
                        SendError(connection, "EXPECTED_PLAYER_ACTION");
                        connection.SendMessage(BuildTurnMessage());
                        continue;
                    }

                    if (actionMessage.Action == null)
                    {
                        SendError(connection, "INVALID_ACTION");
                        connection.SendMessage(BuildTurnMessage());
                        continue;
                    }

                    commandMenu.Execute(actionMessage, game.CurrentRobot);

                    try
                    {
                        Message result = ExecuteAction(actionMessage.Action.Value);
                        ShowResult(result);
                        connection.SendMessage(result);
                    }
                    catch (InvalidOperationException exception)
                    {
                        SendError(connection, exception.Message);
                        connection.SendMessage(BuildTurnMessage());
                        continue;
                    }
                }

                if (game.CheckGameEnded())
                {
                    Message endMessage = BuildEndMessage();
                    connection.SendMessage(endMessage);

                    if (game.HasEscaped)
                    {
                        view.ShowMessage("\n--- FIN DE PARTIE ---");
                        view.ShowMessage("Aucun gagnant, car un robot a réussi à fuir !");
                        Logger.Information("Partie terminée par une fuite");
                    }
                    else
                    {
                        view.ShowWinner(game.GetWinner());
                        Logger.Information($"Partie terminée, avec comme gagnant : {(game.GetWinner().IsHost ? "HOTE" : "CLIENT")}");
                    }

                    return;
                }

                game.NextTurn();
                connection.SendMessage(BuildTurnMessage());
            }
        }

        public void JoinGame()
        {
            isHost = false;
            string ipAddress = view.AskIpAddress();
            int port = view.AskPort();
            socketClient = new SocketClient();

            try
            {
                view.ShowMessage($"\nConnexion à {ipAddress}:{port}...");
                socketClient.ConnectToServer(ipAddress, port);
                view.ShowMessage("Connexion au serveur réussie !");

                ConnectionHandler connection = socketClient.Connection!;
                connection.SendMessage(MessageHelper.BuildMessage(MessageType.PLAYER_JOIN));

                Message welcome = connection.ReceiveMessage();
                if (welcome.Type == MessageType.SERVER_BUSY)
                {
                    view.ShowMessage("Le serveur est déjà occupé !");
                    return;
                }

                if (welcome.Type != MessageType.WELCOME)
                {
                    throw new InvalidDataException("Le serveur n'a pas envoyé WELCOME !");
                }

                bool replay = true;
                while (replay)
                {
                    SendClientConfiguration(connection);
                    RunClientCombat(connection);

                    replay = view.AskPlayerReplay();
                    if (replay)
                    {
                        connection.SendMessage(MessageHelper.BuildMessage(MessageType.PLAYER_REPLAY));
                        Message replayResponse = connection.ReceiveMessage();
                        if (replayResponse.Type != MessageType.PLAYER_REPLAY || replayResponse.Data != "OK")
                        {
                            throw new InvalidDataException("Réponse REPLAY invalide !");
                        }
                    }
                    else
                    {
                        connection.SendMessage(MessageHelper.BuildMessage(MessageType.QUIT));
                        Message quitResponse = connection.ReceiveMessage();
                        if (quitResponse.Type == MessageType.QUIT)
                        {
                            view.ShowMessage("Déconnexion confirmée par le serveur !");
                        }
                    }
                }
            }
            catch (FormatException)
            {
                view.ShowMessage("L'adresse IP saisie n'est pas valide !");
            }
            catch (SocketException exception)
            {
                view.ShowMessage($"Connexion interrompue : {exception.Message}");
            }
            catch (IOException exception)
            {
                view.ShowMessage($"Erreur de protocole : {exception.Message}");
            }
            finally
            {
                socketClient.Exit();
            }
        }

        private void SendClientConfiguration(ConnectionHandler connection)
        {
            bool accepted = false;

            while (!accepted)
            {
                RobotConfig playerConfig = view.AskPlayerConfig();
                Message message = MessageHelper.BuildMessage(MessageType.ROBOT_CONFIG, status: GameStatus.WAITING_FOR_PLAYER_CONFIG, data: MessageHelper.SerializeRobotConfig(playerConfig));

                connection.SendMessage(message);
                Message response = connection.ReceiveMessage();

                if (response.Type == MessageType.ROBOT_CONFIG_OK)
                {
                    commandMenu.Execute(response, null);
                    accepted = true;
                }
                else if (response.Type == MessageType.ERROR)
                {
                    view.ShowMessage($"Configuration refusée : {response.Data}");
                }
                else
                {
                    throw new InvalidDataException("Réponse inattendue à la configuration !");
                }
            }
        }

        private void RunClientCombat(ConnectionHandler connection)
        {
            bool ended = false;

            while (!ended)
            {
                Message message = connection.ReceiveMessage();

                switch (message.Type)
                {
                    case MessageType.GAME_START:
                        ShowStart(message);
                        break;

                    case MessageType.TURN:
                        if (message.Data == "CLIENT")
                        {
                            view.ShowMessage("\n--- Votre tour ---");
                            GameAction action = view.AskPlayerAction();
                            connection.SendMessage(MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, action, GameStatus.PLAYING));
                        }
                        else
                        {
                            view.ShowMessage("\n--- Tour de l'hôte ---");
                        }
                        break;

                    case MessageType.PLAYER_RESULT:
                        commandMenu.Execute(message, null);
                        break;

                    case MessageType.ERROR:
                        commandMenu.Execute(message, null);
                        break;

                    case MessageType.GAME_END:
                        ShowEnd(message);
                        ended = true;
                        break;

                    default:
                        throw new InvalidDataException($"Message inattendu : {message.Type}");
                }
            }
        }

        public Message ExecuteAction(GameAction action)
        {
            if (game == null || game.Status != GameStatus.PLAYING)
            {
                throw new InvalidOperationException("Aucune partie active !");
            }

            Robot actor = game.CurrentRobot;
            Robot target = game.OpponentRobot;
            int damage = 0;
            bool actionCompleted = true;

            switch (action)
            {
                case GameAction.ATTACK:
                    damage = target.ReceiveDamage(actor.Attack());
                    break;

                case GameAction.ATTACK_PUISSANCE:
                    if (actor.GetStatValue(StatsType.ENERGY) < 2)
                    {
                        throw new InvalidOperationException("INVALID_ACTION");
                    }

                    damage = target.ReceiveDamage(actor.AttackWithPower());
                    break;

                case GameAction.DEFENSE:
                    actor.Defend();
                    break;

                case GameAction.RECHARGE:
                    actor.Recharge();
                    break;

                case GameAction.REPAIR:
                    if (actor.GetStatValue(StatsType.ENERGY) < 1)
                    {
                        throw new InvalidOperationException("INVALID_ACTION");
                    }

                    actor.Repair();
                    break;

                case GameAction.DODGE:
                    actionCompleted = actor.Dodge();
                    break;

                case GameAction.ESCAPE:
                    actionCompleted = actor.Escape();

                    if(actionCompleted)
                    {
                        game.EscapeGame();
                    }
                    break;

                default:
                    throw new InvalidOperationException("Action invalide !");
            }

            string actorName = actor.IsHost ? "HOTE" : "CLIENT";
            string data = string.Join(';', actorName, action, actionCompleted, damage,
                game.Robots[0].GetStatValue(StatsType.HP),
                game.Robots[1].GetStatValue(StatsType.HP),
                game.Robots[0].GetStatValue(StatsType.ENERGY),
                game.Robots[1].GetStatValue(StatsType.ENERGY));

            return MessageHelper.BuildMessage(MessageType.PLAYER_RESULT, action, game.CheckGameEnded() ? GameStatus.END_GAME : GameStatus.PLAYING, data);
        }

        public GameStatus GetGameStatus()
        {
            return game?.Status ?? GameStatus.WAITING_FOR_PLAYER;
        }

        public void OnMessageReceived(Message message, Robot? from)
        {
            commandMenu.Execute(message, from);
        }

        private GameAction AskValidAction(Robot robot)
        {
            while (true)
            {
                GameAction action = view.AskPlayerAction();
                if (action == GameAction.ATTACK_PUISSANCE && robot.GetStatValue(StatsType.ENERGY) < 2)
                {
                    view.ShowMessage("Énergie insuffisante => Choisissez une autre action !");
                    continue;
                }
                if (action == GameAction.REPAIR && robot.GetStatValue(StatsType.ENERGY) < 1)
                {
                    view.ShowMessage("Énergie insuffisante => Choisissez une autre action !");
                    continue;
                }
                return action;
            }
        }

        private Message BuildStartMessage()
        {
            if (game == null) throw new InvalidOperationException();

            string data = string.Join(';',
                game.Robots[0].GetStatValue(StatsType.HP),
                game.Robots[1].GetStatValue(StatsType.HP),
                game.Robots[0].GetStatValue(StatsType.ENERGY),
                game.Robots[1].GetStatValue(StatsType.ENERGY));

            return MessageHelper.BuildMessage(MessageType.GAME_START, status: GameStatus.PLAYING, data: data);
        }

        private Message BuildTurnMessage()
        {
            if (game == null) throw new InvalidOperationException();
            return MessageHelper.BuildMessage(MessageType.TURN, status: GameStatus.PLAYING, data: game.CurrentRobot.IsHost ? "HOTE" : "CLIENT");
        }

        private Message BuildEndMessage()
        {
            if (game == null) throw new InvalidOperationException();
            string winner = game.HasEscaped ? "AUCUN" : game.GetWinner().IsHost ? "HOTE" : "CLIENT";

            string data = string.Join(';', winner,
                game.Robots[0].GetStatValue(StatsType.HP),
                game.Robots[1].GetStatValue(StatsType.HP),
                game.Robots[0].GetStatValue(StatsType.ENERGY),
                game.Robots[1].GetStatValue(StatsType.ENERGY));

            return MessageHelper.BuildMessage(MessageType.GAME_END, status: GameStatus.END_GAME, data: data);
        }

        private void SendError(ConnectionHandler connection, string code)
        {
            connection.SendMessage(MessageHelper.BuildMessage(MessageType.ERROR, status: GetGameStatus(), data: code));
        }

        private void ShowStart(Message message)
        {
            view.ShowPlayerRole(isHost);

            string[] values = message.Data.Split(';');
            if (values.Length == 4)
            {
                view.ShowMessage($"\n--- DÉBUT DE LA PARTIE ---" +
                    $"\nHP hôte : {values[0]} | HP client : {values[1]}" +
                    $"\nÉnergie hôte : {values[2]} | Énergie client : {values[3]}");
            }
        }

        private void ShowResult(Message message)
        {
            string[] values = message.Data.Split(';');
            if (values.Length != 8)
            {
                view.ShowMessage("Résultat reçu invalide !");
                Logger.Information("Résultat invalide reçu : " + message.Data);
                return;
            }

            view.ShowMessage($"\n{values[0]} => {values[1]} | Réussite : {values[2]}| Dégâts infligés : {values[3]}");
            view.ShowMessage($"HP : Hôte={values[4]}, Client={values[5]} | Énergie : Hôte={values[6]}, Client={values[7]}");
        }

        private void ShowEnd(Message message)
        {
            string[] values = message.Data.Split(';');
            if (values.Length == 5)
            {
                if (values[0] == "AUCUN")
                {
                    view.ShowMessage($"\n--- FIN DE PARTIE ---" +
                    $"\nAucun gagnant, car un robot a réussi à fuir !" +
                    $"\nHP finaux : Hôte={values[1]}, Client={values[2]}");
                }
                else
                {
                    view.ShowMessage($"\n--- FIN DE PARTIE ---" +
                    $"\nGagnant : {values[0]}" +
                    $"\nHP finaux : Hôte={values[1]}, Client={values[2]}");
                }
            }
            else
            {
                view.ShowMessage("Message de fin de partie invalide !");
                return;
            }
        }
    }
}
