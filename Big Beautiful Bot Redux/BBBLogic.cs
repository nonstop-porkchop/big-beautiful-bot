using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BBB.BotFunctions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BBB
{
    /// <summary>
    ///     This is BBBs unique pipeline.
    /// </summary>
    internal class BBBLogic
    {
        private const string UserInputExceptionMessageTemplate = ":warning: {0}";

        private readonly BotData _botData;
        private readonly string _prefix;
        private readonly ILogger<DiscordBotHost> _logger;
        private readonly RoleManager _roleManager;
        private readonly WeightLog _weightLog;

        public BBBLogic(string prefix, ILogger<DiscordBotHost> logger)
        {
            _prefix = prefix;
            _logger = logger;
            _botData = new BotData();
            _roleManager = new RoleManager(_botData);
            _weightLog = new WeightLog(_botData);
        }

        public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            try
            {
                var emoteString = reaction.Emote.ToString();
                var roleReaction = await _botData.GetRoleReaction(message.Id, emoteString);
                if (roleReaction != null) await RoleManager.ApplyRoleReaction(reaction, roleReaction, message, true);
                else _logger.LogDebug("Received {0} reaction on message id {1} but found no corresponding role to apply.", emoteString, message.Id);
            }
            catch (UserInputException e)
            {
                await (await message.GetOrDownloadAsync()).Channel.SendMessageAsync(string.Format(UserInputExceptionMessageTemplate, e.Message));
            }
        }

        public async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
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

        public async Task HandleMessage(SocketMessage message)
        {
            try
            {
                await HandleCommands(message);
            }
            catch (UserInputException e)
            {
                await message.Channel.SendMessageAsync(string.Format(UserInputExceptionMessageTemplate, e.Message));
            }
        }

        private async Task HandleCommands(SocketMessage message)
        {
            if (message.Content.StartsWith(_prefix))
            {
                var messageSansPrefix = message.Content.TrimStart(_prefix.ToCharArray());
                var messageElements = Regex.Matches(messageSansPrefix, @"[\""].+?[\""]|[^ ]+").Select(x => x.Value.Trim('"')).ToList();

                var commandName = messageElements.FirstOrDefault() ?? throw new UserInputException("There was no command specified.");
                var commandArgs = messageElements.Skip(1).ToList();

                if (commandName.Equals("ping", StringComparison.InvariantCultureIgnoreCase)) await Ping(message);
                if (commandName.Equals("offer", StringComparison.InvariantCultureIgnoreCase)) await _roleManager.OfferRoles(message, commandArgs);
                if (commandName.Equals("convert", StringComparison.InvariantCultureIgnoreCase)) await WeightConverter.ConvertWeight(message, commandArgs);
                if (commandName.Equals("logWeight", StringComparison.InvariantCultureIgnoreCase)) await _weightLog.LogWeight(message, commandArgs);
                if (commandName.Equals("setWelcome", StringComparison.InvariantCultureIgnoreCase)) await SetGuildWelcome(message, messageSansPrefix["setWelcome".Length..]);
                if (commandName.Equals("leaderboard", StringComparison.InvariantCultureIgnoreCase)) await _weightLog.GetLeaderboard(message, commandArgs);
            }
        }

        private async Task SetGuildWelcome(SocketMessage message, string messageText)
        {
            var guildUser = (IGuildUser) message.Author;
            if (!guildUser.GuildPermissions.Administrator) throw new UserInputException("You must be an administrator to set a welcome message.");
            var messageChannel = (SocketGuildChannel)message.Channel;
            await _botData.SetGuildWelcome(messageChannel.Guild.Id, messageText.Trim());
            await message.Channel.SendMessageAsync("The welcome message was set :white_check_mark:");
        }

        private static Task<RestUserMessage> Ping(SocketMessage message) => message.Channel.SendMessageAsync((DateTimeOffset.Now - message.Timestamp).TotalMilliseconds.ToString("0ms"));

        public async Task HandleUserJoin(SocketGuildUser socketGuildUser)
        {
            _logger.LogInformation($"User {socketGuildUser.Username} joined {socketGuildUser.Guild.Name}, fetching welcome message...");
            
            var welcome = await _botData.GetGuildWelcome(socketGuildUser.Guild.Id);
            if (welcome is null)
            {
                _logger.LogDebug($"Welcome message for {nameof(socketGuildUser.Guild.Id)} was not found.");
                return;
            }

            var welcomeMessage = string.Format(welcome.MessageTemplate, socketGuildUser.Mention);
            var guildDefaultChannel = socketGuildUser.Guild.DefaultChannel;
            _logger.LogDebug($@"Greeting with: ""{welcomeMessage}"" in {guildDefaultChannel.Name}.");
            
            await guildDefaultChannel.SendMessageAsync(welcomeMessage);
        }
    }
}