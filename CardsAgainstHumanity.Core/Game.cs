using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Game
    {
        public List<Player> Players { get; } = new List<Player>();

        public GameState State { get; private set; }

        public Player Judge { get; private set; }
    }

    public enum GameState
    {
        Inactive,
        PlayingCards,
        Judging
    }
}
