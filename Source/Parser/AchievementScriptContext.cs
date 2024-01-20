﻿using RATools.Data;
using System.Collections.Generic;

namespace RATools.Parser
{
    public class AchievementScriptContext
    {
        public Dictionary<Achievement, int> Achievements { get; set; }
        public Dictionary<Leaderboard, int> Leaderboards { get; set; }
        public RichPresenceBuilder RichPresence { get; set; }
    }
}
