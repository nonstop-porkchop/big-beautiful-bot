using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Big_Beautiful_Bot_Redux
{
    internal static class WeightConverter
    {
        public const double LbsWeightConversionConstant = 2.20462262185;
        public const double StWeightConversionConstant = 6.35029;

        public static async Task ConvertWeight(SocketMessage message, ICollection<string> commandArgs)
        {
            if (commandArgs.Count != 1) throw new UserInputException($"Expected 1 argument, got {commandArgs.Count}.");
            var single = commandArgs.Single();

            var isKg = Regex.Match(single, "kgs?$").Success;
            var isLbs = Regex.Match(single, "lbs?$").Success;
            var isSt = Regex.Match(single, "st$").Success;
            if (!isKg && !isLbs && !isSt) throw new UserInputException("Couldn't determine input units.");

            var numberString = Regex.Match(single, @"^\d+").Value;
            if (!double.TryParse(numberString, out var number)) throw new Exception("Failed to parse number.");

            if (isKg)
            {
                var lbs = Math.Round(number * LbsWeightConversionConstant, 1);
                await message.Channel.SendMessageAsync($"{number}kg is {lbs}lbs.");
            }

            if (isLbs)
            {
                var kg = Math.Round(number / LbsWeightConversionConstant, 1);
                await message.Channel.SendMessageAsync($"{number}lbs is {kg}kg.");
            }

            if (isSt)
            {
                var kg = Math.Round(number * StWeightConversionConstant, 1);
                await message.Channel.SendMessageAsync($"{number}st is {kg}kg");
            }
        }
    }
}