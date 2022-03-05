using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BBB.BotFunctions;

internal static class WeightConverter
{
    public const double LbsWeightConversionConstant = 2.20462262185;
    public const double StWeightConversionConstant = 6.35029;

    public static async Task ConvertWeight(SocketSlashCommand message)
    {
        if (message.Data.Options.Count != 1) throw new UserInputException($"Expected 1 argument, got {message.Data.Options.Count}.");
        var single = (string)message.Data.Options.Single().Value;

        var isKg = Regex.Match(single, "kgs?$").Success;
        var isLbs = Regex.Match(single, "lbs?$").Success;
        var isSt = Regex.Match(single, "st$").Success;
        if (!isKg && !isLbs && !isSt) throw new UserInputException("Couldn't determine input units.");

        var numberString = Regex.Match(single, @"^\d+").Value;
        if (!double.TryParse(numberString, out var number)) throw new Exception("Failed to parse number.");

        if (isKg)
        {
            var lbs = Math.Round(number * LbsWeightConversionConstant, 1);
            await message.RespondAsync($"{number}kg is {lbs}lbs.");
        }

        if (isLbs)
        {
            var kg = Math.Round(number / LbsWeightConversionConstant, 1);
            await message.RespondAsync($"{number}lbs is {kg}kg.");
        }

        if (isSt)
        {
            var kg = Math.Round(number * StWeightConversionConstant, 1);
            await message.RespondAsync($"{number}st is {kg}kg");
        }
    }
}