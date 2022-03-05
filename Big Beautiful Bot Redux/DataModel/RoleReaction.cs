using JetBrains.Annotations;
using ServiceStack.DataAnnotations;

namespace BBB.DataModel;

internal class RoleReaction
{
    [UsedImplicitly] [AutoIncrement] public long RoleReactionId { get; set; }
    [UsedImplicitly] public ulong Role { get; set; }
    [UsedImplicitly] public string Reaction { get; set; }
    [UsedImplicitly] public ulong OfferingMessageId { get; set; }
}