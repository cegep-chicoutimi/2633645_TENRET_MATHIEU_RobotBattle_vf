using RobotBattle.Models;

namespace RobotBattle.Views
{
    internal interface IGameView
    {
        string AskPlayerType();
        string AskIpAddress();
        int AskPort();
        void ShowPlayerRole(bool isHost);
        void ShowMessage(string message);
        void ShowWinner(Robot robot);
        RobotConfig AskPlayerConfig();
        GameAction AskPlayerAction();
        bool AskPlayerReplay();
    }
}
