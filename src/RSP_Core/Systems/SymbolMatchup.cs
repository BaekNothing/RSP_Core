using RSP_Core.Models;

namespace RSP_Core.Systems
{
    /// <summary>
    /// Handles symbol matchup logic
    /// Square > Triangle, Triangle > Circle, Circle > Square
    /// </summary>
    public static class SymbolMatchup
    {
        public static CombatOutcome DetermineOutcome(SymbolType player, SymbolType enemy)
        {
            if (player == enemy)
            {
                return CombatOutcome.Draw;
            }

            switch (player)
            {
                case SymbolType.Square:
                    return enemy == SymbolType.Triangle ? CombatOutcome.Win : CombatOutcome.Lose;

                case SymbolType.Triangle:
                    return enemy == SymbolType.Circle ? CombatOutcome.Win : CombatOutcome.Lose;

                case SymbolType.Circle:
                    return enemy == SymbolType.Square ? CombatOutcome.Win : CombatOutcome.Lose;

                default:
                    return CombatOutcome.Draw;
            }
        }
    }
}
