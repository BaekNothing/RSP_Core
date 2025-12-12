namespace RSP_Core.Models
{
    /// <summary>
    /// Reference to an effect with optional overrides
    /// </summary>
    public class EffectRef
    {
        public string EffectId { get; set; }
        public int? ValueOverride { get; set; }
        public string? Formula { get; set; }

        public EffectRef(string effectId, int? valueOverride = null, string? formula = null)
        {
            EffectId = effectId;
            ValueOverride = valueOverride;
            Formula = formula;
        }
    }
}
