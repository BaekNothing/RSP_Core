namespace RSP_Core.Models
{
    /// <summary>
    /// Result of symbol matchup
    /// </summary>
    public enum CombatOutcome
    {
        /// <summary>
        /// Player wins matchup - base + win effects execute, enemy negated
        /// </summary>
        Win,
        
        /// <summary>
        /// Draw - base effects execute, enemy attacks
        /// </summary>
        Draw,
        
        /// <summary>
        /// Player loses - no effects execute, enemy attacks
        /// </summary>
        Lose
    }
}
