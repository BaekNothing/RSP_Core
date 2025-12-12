using Xunit;
using RSP_Core.Entities;

namespace RSP_Core.Tests.Entities
{
    public class CardTests
    {
        [Fact]
        public void Card_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var card = new Card("card1", "Fire Blast", 5, 2, 3, "A powerful fire attack");

            // Assert
            Assert.Equal("card1", card.Id);
            Assert.Equal("Fire Blast", card.Name);
            Assert.Equal(5, card.Attack);
            Assert.Equal(2, card.Defense);
            Assert.Equal(3, card.Cost);
            Assert.Equal("A powerful fire attack", card.Description);
        }

        [Fact]
        public void Card_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var card = new Card("card1", "Fire Blast", 5, 2, 3);

            // Act
            var result = card.ToString();

            // Assert
            Assert.Equal("Fire Blast (ATK: 5, DEF: 2, Cost: 3)", result);
        }
    }
}
