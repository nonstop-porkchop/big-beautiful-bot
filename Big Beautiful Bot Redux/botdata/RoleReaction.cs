using ServiceStack.DataAnnotations;

namespace Big_Beautiful_Bot_Redux
{
    internal class RoleReaction
    {
        [AutoIncrement] public long RoleReactionId { get; set; }
        public ulong Role { get; set; }
        public string Reaction { get; set; }
        public ulong OfferingMessageId { get; set; }
    }
}