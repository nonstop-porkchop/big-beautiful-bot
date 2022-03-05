using JetBrains.Annotations;
using ServiceStack.DataAnnotations;

namespace BBB.DataModel;

internal class GuildWelcome
{
    [UsedImplicitly] [AutoIncrement] public long GuildWelcomeId { get; set; }
    public ulong GuildId { get; set; }
    public string MessageTemplate { get; set; }
}