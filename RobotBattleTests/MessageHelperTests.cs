namespace RobotCombatTest;

using RobotBattle.Messages;
using RobotBattle.Models;


[TestClass]
public class MessageHelperTest
{

    [TestMethod]

    public void ShouldBuildActionAttackMessage()

    {

        var expected = "{\"messageType\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";


        var actual = MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");
        var messageString = MessageHelper.SerializeMessage(actual);
        Assert.AreEqual(expected, messageString);

    }


    [TestMethod]

    public void ShouldParseActionAttackMessage()

    {

        var expected = new Message
        {

            Type = MessageType.PLAYER_ACTION,

            Action = GameAction.ATTACK,

            Data = "Test data",

            Status = GameStatus.PLAYING

        };

        var actual = MessageHelper.ParseMessage("""

            {"messageType": 8,"action": 0,"data": "Test data","status": 3}      
            """);


        Assert.AreEqual(expected, actual);

    }

}