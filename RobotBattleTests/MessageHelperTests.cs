using RobotBattle.Messages;
using RobotBattle.Models;

namespace RobotBattle.Tests
{
    [TestClass]
    public class MessageHelperTests
    {
        [TestMethod]
        public void ShouldBuildActionAttackMessage()
        {
            string expected = "{\"type\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";

            Message message = MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");
            string actual = MessageHelper.SerializeMessage(message);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ShouldParseActionAttackMessage()
        {
            string serializedMessage = "{\"type\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";

            Message actual = MessageHelper.ParseMessage(serializedMessage);

            Assert.AreEqual(MessageType.PLAYER_ACTION, actual.Type);
            Assert.AreEqual(GameAction.ATTACK, actual.Action);
            Assert.AreEqual(GameStatus.PLAYING, actual.Status);
            Assert.AreEqual("Test data", actual.Data);
        }

        [TestMethod]
        public void ShouldSerializeAndParseRobotConfig()
        {
            RobotConfig config = new RobotConfig(5, 3, 2);

            string serializedConfig = MessageHelper.SerializeRobotConfig(config);
            RobotConfig parsedConfig = MessageHelper.ParseRobotConfig(serializedConfig);

            Assert.AreEqual("5;3;2", serializedConfig);
            Assert.AreEqual(5, parsedConfig.HpPoints);
            Assert.AreEqual(3, parsedConfig.ArmorPoints);
            Assert.AreEqual(2, parsedConfig.DamagePoints);
        }

        [TestMethod]
        public void ParseRobotConfig_WithInvalidData_ShouldThrowException()
        {
            Assert.ThrowsExactly<InvalidDataException>(() => MessageHelper.ParseRobotConfig("5;3"));
        }
    }
}