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
    }
}
