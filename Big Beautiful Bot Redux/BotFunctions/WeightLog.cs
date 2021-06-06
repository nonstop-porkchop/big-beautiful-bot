using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Big_Beautiful_Bot_Redux
{
    internal class WeightLog
    {
        private readonly BotData _botData;

        public WeightLog(BotData botData) => _botData = botData;

        public async Task LogWeight(SocketMessage message, List<string> commandArgs)
        {
            if (commandArgs.Count != 1) throw new UserInputException($"Expected 1 argument but got {commandArgs.Count}.");
            var weight = commandArgs.Single();
            var regexMatch = Regex.Match(weight, @"^(\d+\.?\d*)((kgs?)|(lbs?)|(st))$").Groups;

            var hasNumber = regexMatch[1].Success;
            if (!hasNumber) throw new UserInputException("Invalid number.");
            var number = double.Parse(regexMatch[1].Value);

            var isKg = regexMatch[3].Success;
            var isLbs = regexMatch[4].Success;
            var isSt = regexMatch[5].Success;

            double kg;

            if (isKg)
            {
                kg = Math.Round(number, 1);
            }
            else if (isLbs)
            {
                kg = Math.Round(number / WeightConverter.LbsWeightConversionConstant, 1);
            }
            else if (isSt)
            {
                kg = Math.Round(number / WeightConverter.StWeightConversionConstant, 1);
            }
            else
            {
                throw new UserInputException("Invalid units.");
            }

            var logEntry = new WeightLogEntry { UserId = message.Author.Id, Weight = kg, TimeStamp = message.CreatedAt.DateTime };
            await _botData.InsertWeightLog(logEntry);
            await message.Channel.SendMessageAsync("Your weight has been updated.");
        }

        public async Task GetLeaderboard(SocketMessage message, List<string> commandArgs)
        {
            var leaderboard = await _botData.GetLeaderboard();
            var users = (await message.Channel.GetUsersAsync().FlattenAsync()).ToList();
            var leaderboardEmbed = new EmbedBuilder { Title = "Weight Leaderboard" };
            var position = 1;
            foreach (var logEntry in leaderboard)
            {
                var user = users.SingleOrDefault(x => x.Id == logEntry.UserId);
                if (user is null) continue;
                leaderboardEmbed.Description += $"**#{position++}**\t{user}\t__{logEntry.Weight}kg__\n";
                if (position == 10) break;
            }

            await message.Channel.SendMessageAsync(embed: leaderboardEmbed.Build());
        }
    }
}