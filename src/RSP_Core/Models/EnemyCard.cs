namespace RSP_Core.Models
{
    /// <summary>
    /// Simple enemy card with symbol and attack value
    /// </summary>
    public class EnemyCard
    {
        public string Id { get; set; }
        public SymbolType Symbol { get; set; }
        public int? Power { get; set; }

        public EnemyCard(string id, SymbolType symbol, int? power = null)
        {
            Id = id;
            Symbol = symbol;
            Power = power;
        }
    }
}
