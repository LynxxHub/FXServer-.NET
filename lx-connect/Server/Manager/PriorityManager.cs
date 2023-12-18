using lx_connect.Server.Model;
using System.Collections.Generic;
using System.Linq;

namespace lx_connect.Server.Manager
{
    public class PriorityManager
    {
        private readonly Config _config;

        public PriorityManager(Config config)
        {
            _config = config;
        }

        // Adds a player's username and Steam ID to the priority list.
        public void AddPriority(string username, string steamID)
        {
            _config.Priority.Add(username, steamID);
        }

        // Removes a player's priority status using Steam ID.
        public void RemovePriority(string steamID)
        {
            _config.Priority.Where(p => p.Value == steamID);
        }

        // Checks if a player has 'God' status by Steam ID in the configuration file. If so, he will join before priority players. 
        public bool IsPlayerGod(string steamID)
        {
            if (steamID != null && _config.Gods.ContainsValue(steamID))
            {
                return true;
            }

            return false;
        }

        // Determines if a player has priority in the queue by Steam ID in the configuration file.
        public bool HasPriority(string steamID)
        {
            if (steamID != null && _config.Priority.ContainsValue(steamID))
            {
                return true;
            }

            return false;
        }

        // Finds the position of a player in the priority queue.
        public int FindPlayerPriorityPosition(List<QueuePlayer> queueList)
        {
            bool hasGod = false;
            for (int i = queueList.Count - 1; i >= 0; i--)
            {
                if (queueList[i].HasPriority)
                {
                    return i + 1;
                }

                if (!hasGod)
                {
                    if (queueList[i].IsGod)
                        hasGod = true;
                }
            }

            if (hasGod)
            {
                return FindGodPriorityPosition(queueList);
            }

            return 0;
        }

        // Finds the position of a 'God' player in the queue.
        public int FindGodPriorityPosition(List<QueuePlayer> queueList)
        {
            for (int i = queueList.Count - 1; i >= 0; i--)
            {
                if (queueList[i].IsGod)
                {
                    return i + 1;
                }
            }
            return 0;
        }
    }
}
