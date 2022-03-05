using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BBB;

internal class DiscordBotHost : IHostedService
{
    private readonly ILogger<DiscordBotHost> _logger;
    private static DiscordSocketClient _client;
    private readonly BBBLogic _logic;

    public DiscordBotHost(IConfiguration config, ILoggerFactory loggerFactory)
    {
        _logger = new Logger<DiscordBotHost>(loggerFactory);
        _client = new DiscordSocketClient();
        _client.LoggedIn += ClientOnLoggedIn;
        _client.MessageReceived += ClientOnMessageReceived;
        _client.ReactionAdded += ClientOnReactionAdded;
        _client.ReactionRemoved += ClientOnReactionRemoved;
        _client.UserJoined += ClientOnUserJoined;
        _client.Log += ClientOnLog;
        _client.LoginAsync(TokenType.Bot, config["Token"]);
        _logic = new BBBLogic(config["Prefix"], new Logger<BBBLogic>(loggerFactory));
    }

    private Task ClientOnLog(LogMessage arg)
    {
        _logger.Log(ToLogLevel(arg.Severity), arg.Exception, "{DiscordClientLogMessage}", arg.Message);
        return Task.CompletedTask;
    }

    private static LogLevel ToLogLevel(LogSeverity argSeverity)
    {
        return argSeverity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(argSeverity), argSeverity, "Couldn't translate LogSeverity to LogLevel.")
        };
    }

    private async Task ClientOnUserJoined(SocketGuildUser arg) => await _logic.HandleUserJoin(arg);

    private async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> cacheable, SocketReaction arg3)
    {
        var message = await arg1.GetOrDownloadAsync();
        if (message.Author.Id.Equals(_client.CurrentUser.Id))
        {
            await _logic.HandleReactionRemoved(arg1, arg3);
        }
    }

    private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> cacheable, SocketReaction arg3)
    {
        var message = await arg1.GetOrDownloadAsync();
        if (message.Author.Id.Equals(_client.CurrentUser.Id))
        {
            await _logic.HandleReactionAdded(arg1, arg3);
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

    public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}