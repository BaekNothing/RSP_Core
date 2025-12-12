using System.Collections.Generic;

namespace RSP_Core.Entities
{
    /// <summary>
    /// Represents a player in the card game with health, deck, and hand
    /// </summary>
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public Deck Deck { get; set; }
        public List<Card> Hand { get; set; }
        public List<Card> DiscardPile { get; set; }

        public bool IsAlive => Health > 0;

        public Player(string id, string name, int maxHealth, int maxEnergy)
        {
            Id = id;
            Name = name;
            Health = maxHealth;
            MaxHealth = maxHealth;
            Energy = maxEnergy;
            MaxEnergy = maxEnergy;
            Deck = new Deck();
            Hand = new List<Card>();
            DiscardPile = new List<Card>();
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health < 0)
            {
                Health = 0;
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public void RestoreEnergy()
        {
            Energy = MaxEnergy;
        }

        public bool CanPlayCard(Card card)
        {
            return Energy >= card.Cost && Hand.Contains(card);
        }

        public void PlayCard(Card card)
        {
            if (CanPlayCard(card))
            {
                Hand.Remove(card);
                Energy -= card.Cost;
                DiscardPile.Add(card);
            }
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Card card = Deck.Draw();
                if (card != null)
                {
                    Hand.Add(card);
                }
                else
                {
                    // Deck is empty, shuffle discard pile back into deck if needed
                    if (DiscardPile.Count > 0)
                    {
                        Deck = new Deck(DiscardPile);
                        DiscardPile.Clear();
                        Deck.Shuffle();
                        
                        card = Deck.Draw();
                        if (card != null)
                        {
                            Hand.Add(card);
                        }
                    }
                    else
                    {
                        // No more cards available (deck and discard pile are empty)
                        break;
                    }
                }
            }
        }
    }
}
