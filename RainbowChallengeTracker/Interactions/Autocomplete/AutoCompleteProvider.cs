﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FuzzySharp;
using RainbowChallengeTracker.DBAccess.Repository;

namespace RainbowChallengeTracker.Interactions.Autocomplete
{
    public class AutoCompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            foreach (var challenge in ChallengeRepository.Challenges)
                choices.Add(new(challenge.Text[..(challenge.Text.Length > 100 ? 100 : challenge.Text.Length)], challenge.ID));
            return Task.FromResult(choices.Where(x =>
            {
                try
                {
                    if (ctx.OptionValue is null)
                        return false;
                    var optionString = ctx.OptionValue?.ToString();
                    if (string.IsNullOrWhiteSpace(optionString))
                        return false;
                    var res = Fuzz.PartialRatio(optionString.ToLower(), x.Name.ToLower());
                    return res > 50;
                }
                catch
                {
                    return false;
                }
            }).Take(25));
        }
    }
}
