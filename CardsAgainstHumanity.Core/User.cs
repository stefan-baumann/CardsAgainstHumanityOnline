using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class User
        : Utils.EquatableBase<User>
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public string Name { get; set; }

        protected override bool IsEqualTo(User other)
        {
            return this.Id == other.Id && this.Token == other.Token;
        }
    }
}
