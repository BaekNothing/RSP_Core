using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Result of resolving a card
    /// </summary>
    public class ResolveResult
    {
        public BattleSnapshot Snapshot { get; set; }
        public CombatOutcome Outcome { get; set; }
        public SymbolType PlayerSymbol { get; set; }
        public SymbolType EnemySymbol { get; set; }
        public int DamageDealtToEnemy { get; set; }
        public int DamageDealtToPlayer { get; set; }
        public List<string> EventTags { get; set; }

        public ResolveResult()
        {
            Snapshot = new BattleSnapshot();
            EventTags = new List<string>();
        }
    }
}
