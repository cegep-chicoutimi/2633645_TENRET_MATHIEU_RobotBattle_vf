using System;
using System.Collections.Generic;
using System.Text;

namespace RobotBattle.Messages
{
    internal class CommandDictionary
    {
        public const string CREATE = "CREATE";
        public const string JOIN = "JOIN";
        public const string WELCOME = "WELCOME";
        public const string SERVER_BUSY = "SERVER_BUSY";
        public const string ROBOT = "ROBOT";
        public const string ROBOT_OK = "ROBOT;OK";
        public const string ERROR = "ERROR";
        public const string START = "START";
        public const string TURN = "TURN";
        public const string ACTION = "ACTION";
        public const string RESULT = "RESULT";
        public const string END = "END";
        public const string REPLAY = "REPLAY";
        public const string REPLAY_OK = "REPLAY;OK";
        public const string QUIT = "QUIT";
        public const string QUIT_OK = "QUIT;OK";

        public static string GetCommand(MessageType type)
        {
            return type switch
            {
                MessageType.PLAYER_JOIN => JOIN,
                MessageType.WELCOME => WELCOME,
                MessageType.SERVER_BUSY => SERVER_BUSY,
                MessageType.ROBOT_CONFIG => ROBOT,
                MessageType.ROBOT_CONFIG_OK => ROBOT_OK,
                MessageType.ERROR => ERROR,
                MessageType.GAME_START => START,
                MessageType.TURN => TURN,
                MessageType.PLAYER_ACTION => ACTION,
                MessageType.PLAYER_RESULT => RESULT,
                MessageType.GAME_END => END,
                MessageType.PLAYER_REPLAY => REPLAY,
                MessageType.QUIT => QUIT,

                _ => throw new ArgumentException("Type de message inconnu !")
            };
        }

        public static MessageType GetMessageType(string command)
        {
            return command switch
            {
                JOIN => MessageType.PLAYER_JOIN,
                WELCOME => MessageType.WELCOME,
                SERVER_BUSY => MessageType.SERVER_BUSY,
                ROBOT => MessageType.ROBOT_CONFIG,
                ROBOT_OK => MessageType.ROBOT_CONFIG_OK,
                ERROR => MessageType.ERROR,
                START => MessageType.GAME_START,
                TURN => MessageType.TURN,
                ACTION => MessageType.PLAYER_ACTION,
                RESULT => MessageType.PLAYER_RESULT,
                END => MessageType.GAME_END,
                REPLAY => MessageType.PLAYER_REPLAY,
                QUIT => MessageType.QUIT,

                _ => throw new ArgumentException($"Commande inconnue : {command} !")
            };
        }
    }
}
