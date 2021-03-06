﻿using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Player
        : Utils.EquatableBase<Player>
    {
        public User User { get; set; }

        public int Points { get; set; } = 0;

        public List<WhiteCard> WhiteCards { get; set; }

        protected override bool IsEqualTo(Player other)
        {
            return this.User == other.User;
        }
    }
}
