using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BBB
{
    internal class DiscordBotHost
    {
        private static DiscordSocketClient _client;
        private readonly BBBLogic _logic;

        public DiscordBotHost(Config config)
        {
            _client = new DiscordSocketClient();
            _client.LoggedIn += ClientOnLoggedIn;
            _client.MessageReceived += ClientOnMessageReceived;
            _client.ReactionAdded += ClientOnReactionAdded;
            _client.ReactionRemoved += ClientOnReactionRemoved;
            _client.LoginAsync(TokenType.Bot, config.Token);
            _logic = new BBBLogic(config);
        }

        private async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var message = await arg1.GetOrDownloadAsync();
            if (message.Author.Id.Equals(_client.CurrentUser.Id))
            {
                await _logic.HandleReactionRemoved(arg1, arg2, arg3);
            }
        }

        /// <summary>
        ///     A task that represents the runtime of the bot. When the bot shuts down, the bot should report this task complete.
        /// </summary>
        private TaskCompletionSource<int> TaskCompletionSource { get; } = new TaskCompletionSource<int>();

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

        public int WaitForShutdown() => TaskCompletionSource.Task.GetAwaiter().GetResult();
    }
}