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

    public async Task OfferRoles(SocketMessage message, IEnumerable<string> args)
    {
        var guildUser = (IGuildUser) message.Author;
        var guild = guildUser.Guild;
        var highestRolePosition = guild.Roles.Where(x => guildUser.RoleIds.Contains(x.Id)).Max(x => x.Position);

        var roleOffering = new EmbedBuilder { Title = "React with the corresponding emoji to receive a roll" };
        var roleCommandArgs = args.ToArray();
        if ((roleCommandArgs.Length & 1) == 1) throw new UserInputException("Expected an even amount of arguments.");
        var roleReactLookup = new List<RoleReaction>();
        for (var i = 0; i < roleCommandArgs.Length; i += 2)
        {
            var roleString = roleCommandArgs[i];
            var stringRoles = guild.Roles.Where(x => x.Name.Equals(roleString) || x.Mention.Equals(roleString)).ToList();

            if (stringRoles.Count > 1) throw new UserInputException($"The role {roleString} is ambiguous, please mention the role explicitly or rename the role and try again.");
            if (!stringRoles.Any()) throw new UserInputException($"Couldn't find a role with the name {roleString}.");
            var role = stringRoles.Single();

            if (role.Position > highestRolePosition && !guildUser.GuildPermissions.Administrator || !guildUser.GuildPermissions.ManageRoles) throw new UserInputException($"User does not have an access level high enough to offer the role {roleString}.");

            var emoteArg = roleCommandArgs[i + 1];
            var isEmote = Emote.TryParse(emoteArg, out var emote);
            if (!isEmote && !Emoji.IsEmoji(emoteArg)) throw new UserInputException($"{emoteArg} is not a valid emoji.");
            var displayEmoji = isEmote ? emote.ToString() : emoteArg;

            roleOffering.Description += $"{displayEmoji} - **{role.Name}**\n";
            roleReactLookup.Add(new RoleReaction { Role = role.Id, Reaction = displayEmoji });
        }

        var embeddedMessage = await message.Channel.SendMessageAsync(embed: roleOffering.Build());
        roleReactLookup.ForEach(x => x.OfferingMessageId = embeddedMessage.Id);
        await _botData.InsertRoleReactions(roleReactLookup);
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