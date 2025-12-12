using Xunit;
using RSP_Core.Models;
using RSP_Core.Systems;

namespace RSP_Core.Tests.Systems
{
    public class SymbolMatchupTests
    {
        [Theory]
        [InlineData(SymbolType.Square, SymbolType.Triangle, CombatOutcome.Win)]
        [InlineData(SymbolType.Triangle, SymbolType.Circle, CombatOutcome.Win)]
        [InlineData(SymbolType.Circle, SymbolType.Square, CombatOutcome.Win)]
        public void SymbolMatchup_WinConditions_ReturnWin(SymbolType player, SymbolType enemy, CombatOutcome expected)
        {
            // Act
            var result = SymbolMatchup.DetermineOutcome(player, enemy);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SymbolType.Square, SymbolType.Circle, CombatOutcome.Lose)]
        [InlineData(SymbolType.Triangle, SymbolType.Square, CombatOutcome.Lose)]
        [InlineData(SymbolType.Circle, SymbolType.Triangle, CombatOutcome.Lose)]
        public void SymbolMatchup_LoseConditions_ReturnLose(SymbolType player, SymbolType enemy, CombatOutcome expected)
        {
            // Act
            var result = SymbolMatchup.DetermineOutcome(player, enemy);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SymbolType.Square, SymbolType.Square)]
        [InlineData(SymbolType.Triangle, SymbolType.Triangle)]
        [InlineData(SymbolType.Circle, SymbolType.Circle)]
        public void SymbolMatchup_SameSymbols_ReturnDraw(SymbolType player, SymbolType enemy)
        {
            // Act
            var result = SymbolMatchup.DetermineOutcome(player, enemy);

            // Assert
            Assert.Equal(CombatOutcome.Draw, result);
        }
    }
}
