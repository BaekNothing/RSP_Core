using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Static definition of a card
    /// </summary>
    public class CardDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SymbolType Symbol { get; set; }
        public CardRole Role { get; set; }
        public int Cost { get; set; }
        public int BaseValue { get; set; }
        public int WinBonusValue { get; set; }
        public List<EffectRef> BaseEffects { get; set; }
        public List<EffectRef> WinEffects { get; set; }
        public string Description { get; set; }

        public CardDefinition()
        {
            Id = string.Empty;
            Name = string.Empty;
            BaseEffects = new List<EffectRef>();
            WinEffects = new List<EffectRef>();
            Description = string.Empty;
        }
    }
}
