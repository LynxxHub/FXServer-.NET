using System;
using System.Collections.Generic;
using System.Text;

namespace lx_connect.Server.Model
{
    public class Config
    {
        public Dictionary<string,string> Gods { get; set; }
        public Dictionary<string,string> Priority { get; set; }
        public string Language { get; set; }
        public int QueueRefreshRate { get; set; }
        public int MaxPlayerCount { get; set; }
        public bool StopHardCap { get; set; }
        public bool DroppedPriority { get; set; }
        public int DroppedPriorityTime { get; set; }
        public bool ShowPriorities { get; set; }
        public bool Debug { get; set; }

    }
}
