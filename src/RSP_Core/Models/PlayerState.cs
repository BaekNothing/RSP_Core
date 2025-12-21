using System.Collections.Generic;

namespace RSP_Core.Models
{
    /// <summary>
    /// Player state during combat
    /// </summary>
    public class PlayerState
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public int DefensePercent { get; set; }
        public List<CardInstance> Deck { get; set; }
        public List<CardInstance> Hand { get; set; }
        public List<CardInstance> Discard { get; set; }
        public Dictionary<string, int> StatusEffects { get; set; }

        public PlayerState()
        {
            Deck = new List<CardInstance>();
            Hand = new List<CardInstance>();
            Discard = new List<CardInstance>();
            StatusEffects = new Dictionary<string, int>();
        }
    }
}
