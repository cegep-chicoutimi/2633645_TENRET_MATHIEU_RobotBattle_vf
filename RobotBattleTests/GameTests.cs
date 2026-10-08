using RobotBattle;
using RobotBattle.Models;

namespace RobotBattle.Tests
{
    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void NextTurn_ShouldChangeCurrentPlayer()
        {
            Config config = new Config();

            RobotConfig robotConfig = new RobotConfig(10, 0, 0);

            Robot host = new Robot(
                true,
                robotConfig.ToStats(config),
                config
            );

            Robot client = new Robot(
                false,
                robotConfig.ToStats(config),
                config
            );

            Game game = new Game(config, host, client);

            game.StartGame();

            Assert.AreEqual(host, game.CurrentRobot);

            game.NextTurn();

            Assert.AreEqual(client, game.CurrentRobot);
        }
    }
}