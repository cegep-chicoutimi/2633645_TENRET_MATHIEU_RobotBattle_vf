using RobotBattle;
using RobotBattle.Models;

namespace RobotBattle.Tests
{
    [TestClass]
    public class RobotTests
    {
        [TestMethod]
        public void ReceiveDamage_ShouldReduceHp()
        {
            Config config = new Config();

            RobotConfig robotConfig = new RobotConfig(0, 0, 10);

            Robot robot = new Robot(
                false,
                robotConfig.ToStats(config),
                config
            );

            robot.ReceiveDamage(20);

            Assert.AreEqual(80, robot.GetStatValue(StatsType.HP));
        }
    }
}