using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BBB.botdata;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace BBB
{
    internal class BotData
    {
        private readonly OrmLiteConnectionFactory _dbFactory;

        public BotData() => _dbFactory = new OrmLiteConnectionFactory("botdata.db", new SqliteOrmLiteDialectProvider());

        public async Task InsertRoleReactions(IEnumerable<RoleReaction> roleReactResponses)
        {
            using var db = await _dbFactory.OpenAsync();
            db.CreateTableIfNotExists<RoleReaction>();
            db.SaveAll(roleReactResponses);
        }

        public async Task<RoleReaction> GetRoleReaction(ulong messageId, string emoteName)
        {
            using var db = await _dbFactory.OpenAsync();
            return db.Single<RoleReaction>(new { OfferingMessageId = messageId, Reaction = emoteName });
        }

        public async Task InsertWeightLog(WeightLogEntry weightLogEntry)
        {
            using var db = await _dbFactory.OpenAsync();
            db.CreateTableIfNotExists<WeightLogEntry>();
            db.Save(weightLogEntry);
        }

        public async Task<IEnumerable<WeightLogEntry>> GetLeaderboard()
        {
            using var db = await _dbFactory.OpenAsync();
            var list = await db.SqlListAsync<WeightLogEntry>(db.From<WeightLogEntry>());
            var groups = list.GroupBy(x => x.UserId);
            return groups.SelectMany(x => x.Where(y => y.TimeStamp == x.Max(z => z.TimeStamp))).ToList().OrderByDescending(x => x.Weight);
        }

        public async Task<GuildWelcome> GetGuildWelcome(ulong guildId)
        {
            using var db = await _dbFactory.OpenAsync();
            return await db.SingleWhereAsync<GuildWelcome>(nameof(GuildWelcome.GuildId), guildId);
        }

        public async Task SetGuildWelcome(ulong guildId, string templateText)
        {
            using var db = await _dbFactory.OpenAsync();
            db.CreateTableIfNotExists<GuildWelcome>();

            var welcome = await GetGuildWelcome(guildId) ?? new GuildWelcome { GuildId = guildId };
            welcome.MessageTemplate = templateText;
            db.Save(welcome);
        }
    }
}