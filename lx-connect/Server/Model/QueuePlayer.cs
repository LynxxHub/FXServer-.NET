using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace lx_connect.Server.Model
{
    internal class QueuePlayer
    {
        public Player Player { get; set; }
        public DateTime JoinedOn { get; set; }
        public string SteamID { get; set; }
        public bool CanJoin { get; set; }

        public bool HasPriority { get; set; }

        public bool IsGod { get; set; }

        public QueuePlayer(Player player, string steamID)
        {
            Player = player;
            JoinedOn = DateTime.UtcNow;
            SteamID = steamID;
            CanJoin = false;
            HasPriority = false;
            IsGod = false;
        }
    }
}
