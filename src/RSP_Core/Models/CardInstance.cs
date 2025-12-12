namespace RSP_Core.Models
{
    /// <summary>
    /// Instance of a card with upgrade information
    /// </summary>
    public class CardInstance
    {
        public string InstanceId { get; set; }
        public CardDefinition Definition { get; set; }
        public int Level { get; set; }
        public int UpgradeCount { get; set; }

        public CardInstance(string instanceId, CardDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition;
            Level = 1;
            UpgradeCount = 0;
        }
    }
}
