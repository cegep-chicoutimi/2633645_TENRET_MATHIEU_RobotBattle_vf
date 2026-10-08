using RobotBattle.Controllers;
using RobotBattle.Models;
using RobotBattle.Views;

namespace RobotBattle.Tests
{
    internal class FakeGameView : IGameView
    {
        public List<string> Messages { get; } = new List<string>();

        public string AskPlayerType()
        {
            return "SERVER";
        }

        public string AskIpAddress()
        {
            return "127.0.0.1";
        }

        public int AskPort()
        {
            return 5000;
        }

        public RobotConfig AskPlayerConfig()
        {
            return new RobotConfig(10, 0, 0);
        }

        public GameAction AskPlayerAction()
        {
            return GameAction.ATTACK;
        }

        public bool AskPlayerReplay()
        {
            return false;
        }

        public void ShowMessage(string message)
        {
            Messages.Add(message);
        }

        public void ShowWinner(Robot robot)
        {
        }
    }

    [TestClass]
    public class GameControllerTests
    {
        [TestMethod]
        public void GetGameStatus_WhenNoGame_ShouldReturnWaitingForPlayer()
        {
            FakeGameView view = new FakeGameView();

            GameController controller = new GameController(view);

            GameStatus status = controller.GetGameStatus();

            Assert.AreEqual(
                GameStatus.WAITING_FOR_PLAYER,
                status
            );
        }
    }
}