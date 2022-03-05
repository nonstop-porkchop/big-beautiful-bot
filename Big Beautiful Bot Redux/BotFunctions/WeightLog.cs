using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BBB.DataModel;
using Discord;
using Discord.WebSocket;

namespace BBB.BotFunctions;

internal class WeightLog
{
    private readonly BotData _botData;

    public WeightLog(BotData botData) => _botData = botData;

    public async Task LogWeight(SocketSlashCommand message)
    {
        if (message.Data.Options.Count != 1) throw new UserInputException($"Expected 1 argument but got {message.Data.Options.Count}.");
        var weight = message.Data.Options.Single();
        var regexMatch = Regex.Match((string)weight.Value, @"^(\d+\.?\d*)((kgs?)|(lbs?)|(st))$").Groups;

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

        var logEntry = new WeightLogEntry { UserId = message.User.Id, Weight = kg, TimeStamp = message.CreatedAt.DateTime };
        await _botData.InsertWeightLog(logEntry);
        await message.RespondAsync("Your weight has been updated.");
    }

    public async Task GetLeaderboard(SocketSlashCommand message)
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

        await message.RespondAsync(embed: leaderboardEmbed.Build());
    }
}