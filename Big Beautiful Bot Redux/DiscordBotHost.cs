using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BBB
{
    internal class DiscordBotHost : IHostedService
    {
        private readonly ILogger<DiscordBotHost> _logger;
        private static DiscordSocketClient _client;
        private readonly BBBLogic _logic;

        public DiscordBotHost(IConfiguration config, ILogger<DiscordBotHost> logger)
        {
            _logger = logger;
            _client = new DiscordSocketClient();
            _client.LoggedIn += ClientOnLoggedIn;
            _client.MessageReceived += ClientOnMessageReceived;
            _client.ReactionAdded += ClientOnReactionAdded;
            _client.ReactionRemoved += ClientOnReactionRemoved;
            _client.UserJoined += ClientOnUserJoined;
            _client.LoginAsync(TokenType.Bot, config["Token"]);
            _logic = new BBBLogic(config["Prefix"], logger);
        }

        private async Task ClientOnUserJoined(SocketGuildUser arg) => await _logic.HandleUserJoin(arg);

        private async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var message = await arg1.GetOrDownloadAsync();
            if (message.Author.Id.Equals(_client.CurrentUser.Id))
            {
                await _logic.HandleReactionRemoved(arg1, arg2, arg3);
            }
        }

        private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var message = await arg1.GetOrDownloadAsync();
            if (message.Author.Id.Equals(_client.CurrentUser.Id))
            {
                await _logic.HandleReactionAdded(arg1, arg2, arg3);
            }
        }

        private async Task ClientOnMessageReceived(SocketMessage message)
        {
            if (Equals(message.Author.Id, _client.CurrentUser.Id))
            {
                // It's myself                
            }
            else
            {
                await _logic.HandleMessage(message);
            }
        }

        private static Task ClientOnLoggedIn() => _client.StartAsync();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => throw new System.NotImplementedException();
    }
}