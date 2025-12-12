using RSP_Core.Models;

namespace RSP_Core.Effects
{
    /// <summary>
    /// Context for effect execution
    /// </summary>
    public class EffectContext
    {
        public BattleSnapshot Snapshot { get; set; }
        public CardInstance SourceCard { get; set; }
        public int Value { get; set; }
        public string Formula { get; set; }
        public CombatOutcome Outcome { get; set; }

        public EffectContext(BattleSnapshot snapshot, CardInstance sourceCard)
        {
            Snapshot = snapshot;
            SourceCard = sourceCard;
            Formula = string.Empty;
        }
    }

    /// <summary>
    /// Delegate for card effect handlers
    /// </summary>
    public delegate void CardEffectDelegate(EffectContext context);
}
