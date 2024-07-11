using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToePlayer
{
    public enum LPref
    {
        Center,
        Side,
        Corner,
        Random
    }
    public class MindSweeper
    {
        public LPref enemyLandminePreference = LPref.Center;
        public LPref ourLandmineStrategy = LPref.Center;
    }
}
