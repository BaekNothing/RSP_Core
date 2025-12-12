using Xunit;
using RSP_Core.Entities;
using System.Collections.Generic;

namespace RSP_Core.Tests.Entities
{
    public class DeckTests
    {
        [Fact]
        public void Deck_Constructor_InitializesEmptyDeck()
        {
            // Arrange & Act
            var deck = new Deck();

            // Assert
            Assert.Equal(0, deck.Count);
        }

        [Fact]
        public void Deck_AddCard_IncreasesCount()
        {
            // Arrange
            var deck = new Deck();
            var card = new Card("card1", "Test Card", 5, 2, 3);

            // Act
            deck.AddCard(card);

            // Assert
            Assert.Equal(1, deck.Count);
        }

        [Fact]
        public void Deck_Draw_ReturnsNullWhenEmpty()
        {
            // Arrange
            var deck = new Deck();

            // Act
            var result = deck.Draw();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Deck_Draw_ReturnsCardAndDecreasesCount()
        {
            // Arrange
            var deck = new Deck();
            var card = new Card("card1", "Test Card", 5, 2, 3);
            deck.AddCard(card);

            // Act
            var drawnCard = deck.Draw();

            // Assert
            Assert.Equal(card, drawnCard);
            Assert.Equal(0, deck.Count);
        }

        [Fact]
        public void Deck_DrawMultiple_ReturnsCorrectNumberOfCards()
        {
            // Arrange
            var deck = new Deck();
            for (int i = 0; i < 5; i++)
            {
                deck.AddCard(new Card($"card{i}", $"Card {i}", 1, 1, 1));
            }

            // Act
            var drawnCards = deck.DrawMultiple(3);

            // Assert
            Assert.Equal(3, drawnCards.Count);
            Assert.Equal(2, deck.Count);
        }

        [Fact]
        public void Deck_Shuffle_ChangesCardOrder()
        {
            // Arrange
            var deck = new Deck();
            var cards = new List<Card>();
            for (int i = 0; i < 20; i++)
            {
                var card = new Card($"card{i}", $"Card {i}", 1, 1, 1);
                deck.AddCard(card);
                cards.Add(card);
            }

            // Act
            deck.Shuffle();

            // Assert - with 20 cards, it's extremely unlikely to be in the same order
            var currentCards = deck.GetCards();
            bool orderChanged = false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != currentCards[i])
                {
                    orderChanged = true;
                    break;
                }
            }
            Assert.True(orderChanged);
        }
    }
}
