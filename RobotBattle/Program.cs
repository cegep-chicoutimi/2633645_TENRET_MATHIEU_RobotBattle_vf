
using RobotBattle.Controllers;
using RobotBattle.Views;
using Serilog;

var startTime = DateTime.Now.ToString("yyMMdd_HHmm");

IGameView view = new ConsoleView();

GameController controller = new GameController(view);

string playerType = view.AskPlayerType();

Log.Logger = new LoggerConfiguration()

    .MinimumLevel.Debug()

    .WriteTo.File(

        path: $"logs/{startTime}_{(playerType)}-.log",

        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")

    .CreateLogger();

if (playerType == "SERVER")
{
    controller.CreateGame();
}
else
{
    controller.JoinGame();
}

Console.WriteLine("\nAppuyez sur Entrée pour fermer...");
Console.ReadLine();
