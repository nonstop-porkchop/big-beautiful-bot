using ServiceStack.DataAnnotations;

namespace BBB.botdata
{
    internal class GuildWelcome
    {
        [AutoIncrement] public long GuildWelcomeId { get; set; }
        public ulong GuildId { get; set; }
        public string MessageTemplate { get; set; }
    }
}