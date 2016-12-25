using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public GameState State { get; set; } = GameState.Inactive;

        public Player Judge { get; set; }

        public CardDatabase Cards { get; set; }
    }

    public enum GameState
    {
        Inactive,
        PlayingCards,
        Judging
    }
}
