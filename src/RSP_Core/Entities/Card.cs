namespace RSP_Core.Entities
{
    /// <summary>
    /// Represents a card in the game with attack and defense properties
    /// </summary>
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }

        public Card(string id, string name, int attack, int defense, int cost, string description = "")
        {
            Id = id;
            Name = name;
            Attack = attack;
            Defense = defense;
            Cost = cost;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Name} (ATK: {Attack}, DEF: {Defense}, Cost: {Cost})";
        }
    }
}
