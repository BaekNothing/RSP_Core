using System;
using System.Collections.Generic;
using System.Linq;

namespace RSP_Core.Entities
{
    /// <summary>
    /// Represents a deck of cards that can be shuffled and drawn from
    /// </summary>
    public class Deck
    {
        private List<Card> cards;
        private Random random;

        public int Count => cards.Count;

        public Deck()
        {
            cards = new List<Card>();
            random = new Random();
        }

        public Deck(List<Card> initialCards) : this()
        {
            cards = new List<Card>(initialCards);
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        public void Shuffle()
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public Card Draw()
        {
            if (cards.Count == 0)
            {
                return null;
            }

            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public List<Card> DrawMultiple(int count)
        {
            List<Card> drawnCards = new List<Card>();
            for (int i = 0; i < count && cards.Count > 0; i++)
            {
                drawnCards.Add(Draw());
            }
            return drawnCards;
        }

        public IReadOnlyList<Card> GetCards()
        {
            return cards.AsReadOnly();
        }
    }
}
