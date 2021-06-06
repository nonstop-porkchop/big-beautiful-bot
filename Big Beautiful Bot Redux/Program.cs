using System;
using System.IO;
using ServiceStack.Text;

namespace BBB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Pull stuff in from the file system
            var configJson = File.ReadAllText("config.json");
            var config = JsonSerializer.DeserializeFromString<Config>(configJson);

            // Make bot and wait for shutdown
            var bot = new DiscordBotHost(config);
            var exitCode = bot.WaitForShutdown();
            Environment.ExitCode = exitCode;
        }
    }
}