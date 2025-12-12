using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Result of advancing to next turn
    /// </summary>
    public class NextTurnResult
    {
        public BattleSnapshot Snapshot { get; set; }
        public List<string> EventTags { get; set; }

        public NextTurnResult()
        {
            Snapshot = new BattleSnapshot();
            EventTags = new List<string>();
        }
    }
}
