using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaLeagueForm.Entities
{
    public enum Side
    {
        Radiant = 0,
        Dire = 1
    }
    class Player
    {
        public string Nickname { get; set; }
        public bool Ready { get; set; }
        public Side Side { get; set; }
        public int Slot { get; set; }

        public ulong UserId { get; set; }
    }
}
