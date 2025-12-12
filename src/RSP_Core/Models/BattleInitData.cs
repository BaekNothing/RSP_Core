namespace RSP_Core.Models
{
    /// <summary>
    /// Initialization data for combat
    /// </summary>
    public class BattleInitData
    {
        public PlayerState InitialPlayerState { get; set; }
        public EnemyState InitialEnemyState { get; set; }
        public int MaxSlotsPerTurn { get; set; }
        public int InitialHandSize { get; set; }

        public BattleInitData()
        {
            InitialPlayerState = new PlayerState();
            InitialEnemyState = new EnemyState();
            MaxSlotsPerTurn = 3;
            InitialHandSize = 5;
        }
    }
}
