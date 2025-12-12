namespace RSP_Core.Battle
{
    /// <summary>
    /// Represents the different states a battle can be in
    /// </summary>
    public enum BattleState
    {
        NotStarted,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat,
        Draw
    }
}
