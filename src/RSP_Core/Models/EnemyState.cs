using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Enemy state during combat
    /// </summary>
    public class EnemyState
    {
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int AttackValue { get; set; }
        public SymbolType? LastSymbol { get; set; }
        public List<EnemyCard> Deck { get; set; }
        public List<EnemyCard> Discard { get; set; }
        public Dictionary<string, int> StatusEffects { get; set; }

        public EnemyState()
        {
            Deck = new List<EnemyCard>();
            Discard = new List<EnemyCard>();
            StatusEffects = new Dictionary<string, int>();
        }
    }
}
