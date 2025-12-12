namespace RSP_Core.Models
{
    /// <summary>
    /// Simple enemy card with symbol and attack value
    /// </summary>
    public class EnemyCard
    {
        public string CardId { get; set; }
        public SymbolType SymbolType { get; set; }
        public int AttackValue { get; set; }

        public EnemyCard(string cardId, SymbolType symbolType, int attackValue)
        {
            CardId = cardId;
            SymbolType = symbolType;
            AttackValue = attackValue;
        }
    }
}
