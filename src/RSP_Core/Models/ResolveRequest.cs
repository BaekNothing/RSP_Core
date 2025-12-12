namespace RSP_Core.Models
{
    /// <summary>
    /// Request to resolve a card in a specific slot
    /// </summary>
    public class ResolveRequest
    {
        public string CardInstanceId { get; set; }
        public int SlotIndex { get; set; }

        public ResolveRequest(string cardInstanceId, int slotIndex)
        {
            CardInstanceId = cardInstanceId;
            SlotIndex = slotIndex;
        }
    }
}
