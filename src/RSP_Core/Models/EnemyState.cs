using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Enemy state during combat
    /// </summary>
    public class EnemyState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int AttackValue { get; set; }
        public SymbolType? LastUsedSymbol { get; set; }
        public List<EnemyCard> Deck { get; set; }
        public List<EnemyCard> Discard { get; set; }
        public Dictionary<string, int> StatusEffects { get; set; }

        public EnemyState()
        {
            Id = string.Empty;
            Name = string.Empty;
            Deck = new List<EnemyCard>();
            Discard = new List<EnemyCard>();
            StatusEffects = new Dictionary<string, int>();
        }
    }
}
