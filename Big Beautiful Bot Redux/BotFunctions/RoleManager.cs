using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BBB.DataModel;
using Discord;
using Discord.WebSocket;
using Emoji = NeoSmart.Unicode.Emoji;

namespace BBB.BotFunctions;

internal class RoleManager
{
    private readonly BotData _botData;

    public RoleManager(BotData botData) => _botData = botData;

    public async Task OfferRoles(SocketSlashCommand message)
    {
        if ((message.Data.Options.Count & 1) == 1) throw new UserInputException("Expected an even amount of arguments.");

        var guildUser = (IGuildUser) message.User;
        var guild = guildUser.Guild;
        var highestRolePosition = guild.Roles.Where(x => guildUser.RoleIds.Contains(x.Id)).Max(x => x.Position);

        var roleOffering = new EmbedBuilder { Title = "React with the corresponding emoji to receive a roll" };
        var roleCommandArgs = message.Data.Options
            .Join(message.Data.Options, GetRoleMatcher, GetEmojiMatcher, ToKeyValuePair)
            .ToDictionary(x => x.Key, x => x.Value);

        var roleReactLookup = new List<RoleReaction>();
        foreach (var (roleOption, emoteOption) in roleCommandArgs)
        {
            var role = (IRole)roleOption.Value;
            if (role.Position > highestRolePosition && !guildUser.GuildPermissions.Administrator || !guildUser.GuildPermissions.ManageRoles) throw new UserInputException($"User does not have an access level high enough to offer the role {roleOption}.");

            var rawEmote = (string)emoteOption.Value;
            var isEmote = Emote.TryParse(rawEmote, out var emote);
            if (!isEmote && !Emoji.IsEmoji(rawEmote)) throw new UserInputException($"{rawEmote} is not a valid emoji.");
            var displayEmoji = isEmote ? emote.ToString() : rawEmote;

            roleOffering.Description += $"{displayEmoji} - **{role.Name}**\n";
            roleReactLookup.Add(new RoleReaction { Role = role.Id, Reaction = displayEmoji });
        }

        await message.RespondAsync(embed: roleOffering.Build());
        var embeddedMessage = await message.GetOriginalResponseAsync();
        roleReactLookup.ForEach(x => x.OfferingMessageId = embeddedMessage.Id);
        await _botData.InsertRoleReactions(roleReactLookup);
    }

    private static KeyValuePair<T1, T2> ToKeyValuePair<T1, T2>(T1 x, T2 y) => new(x, y);

    private static string GetEmojiMatcher(SocketSlashCommandDataOption arg)
    {
        return arg.Type == ApplicationCommandOptionType.Role ? Guid.NewGuid().ToString() : arg.Name.Split('-').Last();
    }

    private static string GetRoleMatcher(SocketSlashCommandDataOption arg)
    {
        return arg.Type != ApplicationCommandOptionType.Role ? Guid.NewGuid().ToString() : arg.Name.Split('-').Last();
    }

    public static async Task ApplyRoleReaction(SocketReaction reaction, RoleReaction roleReaction, Cacheable<IUserMessage, ulong> cacheable, bool added)
    {
        if (reaction.User.IsSpecified)
        {
            var guildBot = (IGuildUser) (await cacheable.GetOrDownloadAsync()).Author;
            var guild = guildBot.Guild;
            var highestRolePosition = guild.Roles.Where(x => guildBot.RoleIds.Contains(x.Id)).Max(x => x.Position);
            var guildUser = (IGuildUser) reaction.User.Value;
            var role = guildUser.Guild.GetRole(roleReaction.Role);
            if (role.Position > highestRolePosition || !guildBot.GuildPermissions.ManageRoles) throw new UserInputException($"The bot is not a high enough rank to grant or revoke role {role}.");

            if (added)
            {
                await guildUser.AddRoleAsync(role);
            }
            else
            {
                await guildUser.RemoveRoleAsync(role);
            }
        }
        else
        {
            throw new Exception("The user who added/removed this reaction was not specified in the API object.");
        }
    }
}