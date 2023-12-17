using lx_connect.Server.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace lx_connect.Server.Manager
{
    internal class PriorityManager
    {
        private readonly Config _config;

        public PriorityManager(Config config)
        {
            _config = config;
        }
        public bool IsPlayerGod(string steamID)
        {
            if (steamID != null && _config.Gods.ContainsValue(steamID))
            {
                return true;
            }

            return false;
        }

        public bool HasPriority(string steamID)
        {
            if (steamID != null && _config.Priority.ContainsValue(steamID))
            {
                return true;
            }

            return false;
        }

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



        //TODO: Implement Add and remove priority and server side commands
    }
}
