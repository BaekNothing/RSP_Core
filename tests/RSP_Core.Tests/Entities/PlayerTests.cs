using Xunit;
using RSP_Core.Entities;

namespace RSP_Core.Tests.Entities
{
    public class PlayerTests
    {
        [Fact]
        public void Player_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var player = new Player("player1", "John", 100, 10);

            // Assert
            Assert.Equal("player1", player.Id);
            Assert.Equal("John", player.Name);
            Assert.Equal(100, player.Health);
            Assert.Equal(100, player.MaxHealth);
            Assert.Equal(10, player.Energy);
            Assert.Equal(10, player.MaxEnergy);
            Assert.NotNull(player.Deck);
            Assert.NotNull(player.Hand);
            Assert.NotNull(player.DiscardPile);
            Assert.True(player.IsAlive);
        }

        [Fact]
        public void Player_TakeDamage_ReducesHealth()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);

            // Act
            player.TakeDamage(30);

            // Assert
            Assert.Equal(70, player.Health);
            Assert.True(player.IsAlive);
        }

        [Fact]
        public void Player_TakeDamage_CannotGoBelowZero()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);

            // Act
            player.TakeDamage(150);

            // Assert
            Assert.Equal(0, player.Health);
            Assert.False(player.IsAlive);
        }

        [Fact]
        public void Player_Heal_IncreasesHealth()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            player.TakeDamage(50);

            // Act
            player.Heal(30);

            // Assert
            Assert.Equal(80, player.Health);
        }

        [Fact]
        public void Player_Heal_CannotExceedMaxHealth()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            player.TakeDamage(20);

            // Act
            player.Heal(50);

            // Assert
            Assert.Equal(100, player.Health);
        }

        [Fact]
        public void Player_RestoreEnergy_SetsToMaxEnergy()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            player.Energy = 3;

            // Act
            player.RestoreEnergy();

            // Assert
            Assert.Equal(10, player.Energy);
        }

        [Fact]
        public void Player_CanPlayCard_ReturnsTrueWhenValid()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            var card = new Card("card1", "Test Card", 5, 2, 3);
            player.Hand.Add(card);

            // Act
            var result = player.CanPlayCard(card);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Player_CanPlayCard_ReturnsFalseWhenNotEnoughEnergy()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            player.Energy = 2;
            var card = new Card("card1", "Test Card", 5, 2, 3);
            player.Hand.Add(card);

            // Act
            var result = player.CanPlayCard(card);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Player_PlayCard_RemovesFromHandAndReducesEnergy()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            var card = new Card("card1", "Test Card", 5, 2, 3);
            player.Hand.Add(card);

            // Act
            player.PlayCard(card);

            // Assert
            Assert.DoesNotContain(card, player.Hand);
            Assert.Equal(7, player.Energy);
            Assert.Contains(card, player.DiscardPile);
        }

        [Fact]
        public void Player_DrawCards_AddsCardsToHand()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            for (int i = 0; i < 5; i++)
            {
                player.Deck.AddCard(new Card($"card{i}", $"Card {i}", 1, 1, 1));
            }

            // Act
            player.DrawCards(3);

            // Assert
            Assert.Equal(3, player.Hand.Count);
            Assert.Equal(2, player.Deck.Count);
        }

        [Fact]
        public void Player_DrawCards_ShufflesDiscardPileWhenDeckEmpty()
        {
            // Arrange
            var player = new Player("player1", "John", 100, 10);
            var card1 = new Card("card1", "Card 1", 1, 1, 1);
            var card2 = new Card("card2", "Card 2", 1, 1, 1);
            player.DiscardPile.Add(card1);
            player.DiscardPile.Add(card2);

            // Act
            player.DrawCards(1);

            // Assert
            Assert.Equal(1, player.Hand.Count);
            Assert.Equal(0, player.DiscardPile.Count);
        }
    }
}
