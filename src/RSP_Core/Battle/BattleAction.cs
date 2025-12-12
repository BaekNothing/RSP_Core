using RSP_Core.Entities;

namespace RSP_Core.Battle
{
    /// <summary>
    /// Represents an action taken during a turn
    /// </summary>
    public class BattleAction
    {
        public Player Actor { get; set; }
        public Player Target { get; set; }
        public Card Card { get; set; }
        public int Damage { get; set; }
        public string Description { get; set; }

        public BattleAction(Player actor, Player target, Card card, int damage, string description)
        {
            Actor = actor;
            Target = target;
            Card = card;
            Damage = damage;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
