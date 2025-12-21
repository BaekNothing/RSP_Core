namespace RSP_Core.Models
{
    /// <summary>
    /// Initialization data for combat
    /// </summary>
    public class BattleInitData
    {
        public PlayerState PlayerState { get; set; }
        public EnemyState EnemyState { get; set; }
        public int? Seed { get; set; }
        public int MaxSlotsPerTurn { get; set; }
        public int InitialHandSize { get; set; }

        public BattleInitData()
        {
            PlayerState = new PlayerState();
            EnemyState = new EnemyState();
            MaxSlotsPerTurn = 3;
            InitialHandSize = 5;
        }
    }
}
