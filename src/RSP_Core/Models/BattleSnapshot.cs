namespace RSP_Core.Models
{
    /// <summary>
    /// Complete battle state snapshot
    /// </summary>
    public class BattleSnapshot
    {
        public PlayerState Player { get; set; }
        public EnemyState Enemy { get; set; }
        public int TurnNumber { get; set; }
        public bool IsBattleEnded { get; set; }
        public bool IsPlayerDead { get; set; }
        public bool IsEnemyDead { get; set; }

        public BattleSnapshot()
        {
            Player = new PlayerState();
            Enemy = new EnemyState();
        }
    }
}
