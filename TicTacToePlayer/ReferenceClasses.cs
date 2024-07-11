using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToePlayer
{
    public class PlayerMove
    {
        public bool DoesTauntOpponent { get; set; }
        public int[] Coordinate { get; set; }
        public string CustomTaunt { get; set; }
    }

    public class Landmine
    {
        public int[] Coordinate { get; set; }
    }
    public enum CurrentGameStatus
    {
        WaitingForOpponent = 0,
        PlayerXTurn = 1,
        PlayerOTurn = 2,
        GameOver = 3
    }
    public class TicTacToeGame
    {
        public string PlayerXName { get; set; }
        public string PlayerOName { get; set; }
        public string RoomCode { get; set; }
        public CurrentGameStatus currentGameStatus { get; set; }
        public char[][] GameBoard { get; set; }
        public string WinnerStatus { get; set; }
        public string CurrentTauntFromX { get; set; }
        public string CurrentTauntFromO { get; set; }
        private bool PlayerXVotesToContinue;
        private bool PlayerOVotesToContinue;
        public int PlayerXVictoryCount { get; set; }
        public int PlayerOVictoryCount { get; set; }
        public int PlayerXForfeitCount { get; set; }
        public int PlayerOForfeitCount { get; set; }
        public int TieGameCount { get; set; }

        public int CurrentRound { get; set; }

        private bool DidPlayerXGoFirst { get; set; }
        private List<string> Taunts = new List<string>
        {
            "Your pitiful attempts at strategy are no match for my royal intellect.",
            "I tire of this peasantry game, I shall end it forthwith.",
            "You dare to play against a king? How quaint.",
            "You shall be forever relegated to the annals of history as the peasant who lost to royalty.",
            "Your plebeian mind is no match for my royal dominance.",
            "I shall grant you the mercy of a swift defeat.",
            "Your tic-tac-toe skills are beneath my notice.",
            "I shall allow you this small victory, peasant, before I crush you.",
            "I am the king and this game is my kingdom, all shall bow down before me.",
            "You challenge me, a king, in this mere game? I shall show you true power.",
            "Thou dost play the game as if thou hadst a brain of pudding.",
            "Thy strategy doth resemble that of a drowning fly.",
            "Thou art but a mere shadow of a player, compared to I.",
            "Thy moves are as predictable as a clock in a church tower.",
            "Thou art like a moth to the flame, for thou cannot resist the lure of defeat.",
            "Thy plays are as weak as a newborn babe.",
            "Thou art as useful to this game as a wooden sword in battle.",
            "Thy plays are as sharp as a bowling ball",
            "You are playing like a bot"
        };
    }

    public class Coordinate
    {
        public Coordinate(int[] point)
        {
            X = point[0];
            Y = point[1];
        }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int[] ToPoint()
        {
            return new int[] { X, Y };
        }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
