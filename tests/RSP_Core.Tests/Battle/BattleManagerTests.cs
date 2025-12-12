using Xunit;
using RSP_Core.Battle;
using RSP_Core.Entities;

namespace RSP_Core.Tests.Battle
{
    public class BattleManagerTests
    {
        private Player CreateTestPlayer(string id, string name)
        {
            var player = new Player(id, name, 100, 10);
            
            // Add some test cards to the deck
            for (int i = 0; i < 10; i++)
            {
                player.Deck.AddCard(new Card($"{id}_card{i}", $"Card {i}", i + 1, i, 2));
            }
            
            return player;
        }

        [Fact]
        public void BattleManager_Constructor_InitializesCorrectly()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");

            // Act
            var battleManager = new BattleManager(player1, player2);

            // Assert
            Assert.Equal(player1, battleManager.Player1);
            Assert.Equal(player2, battleManager.Player2);
            Assert.Equal(BattleState.NotStarted, battleManager.CurrentState);
            Assert.Equal(0, battleManager.TurnCount);
            Assert.Empty(battleManager.ActionHistory);
        }

        [Fact]
        public void BattleManager_StartBattle_InitializesPlayers()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);

            // Act
            battleManager.StartBattle(5);

            // Assert
            Assert.Equal(BattleState.PlayerTurn, battleManager.CurrentState);
            Assert.Equal(1, battleManager.TurnCount);
            Assert.Equal(5, player1.Hand.Count);
            Assert.Equal(5, player2.Hand.Count);
            Assert.Equal(5, player1.Deck.Count);
            Assert.Equal(5, player2.Deck.Count);
        }

        [Fact]
        public void BattleManager_PlayCard_ExecutesActionAndDealsDamage()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(5);

            var card = player1.Hand[0];
            int initialHealth = player2.Health;

            // Act
            var action = battleManager.PlayCard(card);

            // Assert
            Assert.NotNull(action);
            Assert.Equal(player1, action.Actor);
            Assert.Equal(player2, action.Target);
            Assert.Equal(card, action.Card);
            Assert.Equal(initialHealth - card.Attack, player2.Health);
            Assert.DoesNotContain(card, player1.Hand);
        }

        [Fact]
        public void BattleManager_PlayCard_ReturnNullWhenCardNotPlayable()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(5);

            // Create a card that costs more than available energy
            var expensiveCard = new Card("exp", "Expensive", 10, 5, 20);
            player1.Hand.Add(expensiveCard);

            // Act
            var action = battleManager.PlayCard(expensiveCard);

            // Assert
            Assert.Null(action);
        }

        [Fact]
        public void BattleManager_EndTurn_SwitchesPlayers()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(3);

            int initialHandSize = player2.Hand.Count;

            // Act
            battleManager.EndTurn();

            // Assert
            Assert.Equal(BattleState.EnemyTurn, battleManager.CurrentState);
            Assert.Equal(player2, battleManager.GetCurrentPlayer());
            Assert.Equal(player1, battleManager.GetOpponent());
            Assert.Equal(player2.MaxEnergy, player2.Energy);
            Assert.Equal(initialHandSize + 2, player2.Hand.Count); // Drew 2 cards
        }

        [Fact]
        public void BattleManager_EndTurn_IncreasesTurnCountWhenBackToPlayer1()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(3);

            // Act
            battleManager.EndTurn(); // Switch to player 2
            battleManager.EndTurn(); // Switch back to player 1

            // Assert
            Assert.Equal(2, battleManager.TurnCount);
            Assert.Equal(BattleState.PlayerTurn, battleManager.CurrentState);
        }

        [Fact]
        public void BattleManager_CheckBattleEnd_DetectsVictory()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            player2.Health = 5; // Low health
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(5);

            // Find a card with enough attack
            Card strongCard = null;
            foreach (var card in player1.Hand)
            {
                if (card.Attack >= 5)
                {
                    strongCard = card;
                    break;
                }
            }

            if (strongCard == null)
            {
                strongCard = new Card("strong", "Strong", 10, 1, 2);
                player1.Hand.Add(strongCard);
            }

            // Act
            battleManager.PlayCard(strongCard);

            // Assert
            Assert.Equal(BattleState.Victory, battleManager.CurrentState);
            Assert.False(battleManager.IsBattleActive());
            Assert.Equal(player1, battleManager.GetWinner());
        }

        [Fact]
        public void BattleManager_CheckBattleEnd_DetectsDefeat()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            player1.Health = 5; // Low health
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(5);

            // Switch to player 2's turn
            battleManager.EndTurn();

            // Find a card with enough attack
            Card strongCard = null;
            foreach (var card in player2.Hand)
            {
                if (card.Attack >= 5)
                {
                    strongCard = card;
                    break;
                }
            }

            if (strongCard == null)
            {
                strongCard = new Card("strong", "Strong", 10, 1, 2);
                player2.Hand.Add(strongCard);
            }

            // Act
            battleManager.PlayCard(strongCard);

            // Assert
            Assert.Equal(BattleState.Defeat, battleManager.CurrentState);
            Assert.False(battleManager.IsBattleActive());
            Assert.Equal(player2, battleManager.GetWinner());
        }

        [Fact]
        public void BattleManager_IsBattleActive_ReturnsTrueDuringBattle()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);

            // Act
            battleManager.StartBattle(5);

            // Assert
            Assert.True(battleManager.IsBattleActive());
        }

        [Fact]
        public void BattleManager_ActionHistory_TracksAllActions()
        {
            // Arrange
            var player1 = CreateTestPlayer("p1", "Player 1");
            var player2 = CreateTestPlayer("p2", "Player 2");
            var battleManager = new BattleManager(player1, player2);
            battleManager.StartBattle(5);

            // Act
            var card1 = player1.Hand[0];
            battleManager.PlayCard(card1);
            
            var card2 = player1.Hand[0];
            battleManager.PlayCard(card2);

            // Assert
            Assert.Equal(2, battleManager.ActionHistory.Count);
            Assert.Equal(card1, battleManager.ActionHistory[0].Card);
            Assert.Equal(card2, battleManager.ActionHistory[1].Card);
        }
    }
}
