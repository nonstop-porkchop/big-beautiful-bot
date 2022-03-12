using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BBB.BotFunctions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BBB;

/// <summary>
///     This is BBBs unique pipeline.
/// </summary>
internal class BBBLogic
{
    private const string UserInputExceptionMessageTemplate = ":warning: {0}";

    private readonly BotData _botData;
    private readonly ILogger<BBBLogic> _logger;
    private readonly RoleManager _roleManager;
    private readonly WeightLog _weightLog;

    public BBBLogic(ILogger<BBBLogic> logger)
    {
        _logger = logger;
        _botData = new BotData();
        _roleManager = new RoleManager(_botData);
        _weightLog = new WeightLog(_botData);
    }

    public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> message, SocketReaction reaction)
    {
        try
        {
            var emoteString = reaction.Emote.ToString();
            var roleReaction = await _botData.GetRoleReaction(message.Id, emoteString);
            if (roleReaction != null) await RoleManager.ApplyRoleReaction(reaction, roleReaction, message, true);
            else _logger.LogDebug("Received {Emote} reaction on message id {MessageId} but found no corresponding role to apply", emoteString, message.Id);
        }
        catch (UserInputException e)
        {
            await (await message.GetOrDownloadAsync()).Channel.SendMessageAsync(string.Format(UserInputExceptionMessageTemplate, e.Message));
        }
    }

    public async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> message, SocketReaction reaction)
    {
        try
        {
            var roleReaction = await _botData.GetRoleReaction(message.Id, reaction.Emote.ToString());
            if (roleReaction != null) await RoleManager.ApplyRoleReaction(reaction, roleReaction, message, false);
        }
        catch (UserInputException e)
        {
            await (await message.GetOrDownloadAsync()).Channel.SendMessageAsync(string.Format(UserInputExceptionMessageTemplate, e.Message));
        }
    }

    public async Task HandleCommand(SocketSlashCommand message)
    {
        try
        {
            if (message.CommandName.Equals("ping", StringComparison.InvariantCultureIgnoreCase)) await Ping(message);
            if (message.CommandName.Equals("offer", StringComparison.InvariantCultureIgnoreCase)) await _roleManager.OfferRoles(message);
            if (message.CommandName.Equals("convert", StringComparison.InvariantCultureIgnoreCase)) await WeightConverter.ConvertWeight(message);
            if (message.CommandName.Equals("log-weight", StringComparison.InvariantCultureIgnoreCase)) await _weightLog.LogWeight(message);
            if (message.CommandName.Equals("set-welcome", StringComparison.InvariantCultureIgnoreCase)) await SetGuildWelcome(message);
            if (message.CommandName.Equals("leaderboard", StringComparison.InvariantCultureIgnoreCase)) await _weightLog.GetLeaderboard(message);
        }
        catch (UserInputException e)
        {
            await message.RespondAsync(string.Format(UserInputExceptionMessageTemplate, e.Message));
        }
    }

    public static IEnumerable<SlashCommandBuilder> GetCommands()
    {
        yield return new SlashCommandBuilder { Name = "ping", Description = "Poorly estimates the time taken for your command to reach the BBB server" };
        var offerCommandBuilder = new SlashCommandBuilder { Name = "offer", Description = "Creates an embed that allows users to assign their own roles" };
        for (var i = 1; i < 7; i++)
        {
            offerCommandBuilder.AddOption($"role-{i}", ApplicationCommandOptionType.Role, "The role assigned by the proceeding emote", i == 1);
            offerCommandBuilder.AddOption($"emote-{i}", ApplicationCommandOptionType.String, "The emote that will assign the preceding role", i == 1);    
        }
        yield return offerCommandBuilder;
        var convertCommandBuilder = new SlashCommandBuilder { Name = "convert", Description = "Converts units of weight between lbs, kgs and stn" };
        convertCommandBuilder.AddOption("weight", ApplicationCommandOptionType.String, "The weight to convert", true);
        yield return convertCommandBuilder;
        var logWeightCommandBuilder = new SlashCommandBuilder { Name = "log-weight", Description = "Logs your weight to the weight database for use in leaderboard" };
        logWeightCommandBuilder.AddOption("weight", ApplicationCommandOptionType.String, "Your current weight", true);
        yield return logWeightCommandBuilder;
        var setWelcomeCommandBuilder = new SlashCommandBuilder { Name = "set-welcome", Description = "Sets the welcome message for the guild" };
        setWelcomeCommandBuilder.AddOption("greeting", ApplicationCommandOptionType.String, "The greeting to send when a new member joins");
        yield return setWelcomeCommandBuilder;
        yield return new SlashCommandBuilder { Name = "leaderboard", Description = "Displays the weight leaderboard for this guild" };
    }

    private async Task SetGuildWelcome(SocketSlashCommand message)
    {
        var guildUser = (IGuildUser) message.User;
        if (!guildUser.GuildPermissions.Administrator) throw new UserInputException("You must be an administrator to set a welcome message.");
        var messageChannel = (SocketGuildChannel)message.Channel;
        await _botData.SetGuildWelcome(messageChannel.Guild.Id, (string) message.Data.Options.Single().Value);
        await message.RespondAsync("The welcome message was set :white_check_mark:");
    }

    private static Task Ping(SocketInteraction message) => message.RespondAsync((DateTimeOffset.Now - message.CreatedAt).TotalMilliseconds.ToString("0ms"));

    public async Task HandleUserJoin(SocketGuildUser socketGuildUser)
    {
        _logger.LogInformation("User {Username} joined {Guild}, fetching welcome message...", socketGuildUser.Username, socketGuildUser.Guild.Name);
            
        var welcome = await _botData.GetGuildWelcome(socketGuildUser.Guild.Id);
        if (welcome is null)
        {
            _logger.LogDebug($"Welcome message for {nameof(socketGuildUser.Guild.Id)} was not found.");
            return;
        }

        var welcomeMessage = string.Format(welcome.MessageTemplate, socketGuildUser.Mention);
        var guildDefaultChannel = socketGuildUser.Guild.DefaultChannel;
        _logger.LogDebug(@"Greeting with: ""{WelcomeMessage}"" in {Channel}", welcomeMessage, guildDefaultChannel.Name);
            
        await guildDefaultChannel.SendMessageAsync(welcomeMessage);
    }

    public async Task PreparePersistence() => await _botData.EnsureTablesExist();
}