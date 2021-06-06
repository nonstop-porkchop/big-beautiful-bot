using ServiceStack.DataAnnotations;

namespace BBB.botdata
{
    internal class RoleReaction
    {
        [AutoIncrement] public long RoleReactionId { get; set; }
        public ulong Role { get; set; }
        public string Reaction { get; set; }
        public ulong OfferingMessageId { get; set; }
    }
}