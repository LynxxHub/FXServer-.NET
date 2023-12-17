using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace lx_connect.Server
{
    internal class QueuePlayer
    {
        public Player Player { get; set; }
        public DateTime JoinedOn { get; set; }
        public bool CanJoin { get; set; }

        public QueuePlayer(Player player, DateTime joinedOn, bool canJoin)
        {
            Player = player;
            JoinedOn = joinedOn;
            CanJoin = canJoin;
        }
    }
}
