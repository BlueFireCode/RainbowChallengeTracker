using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using RainbowChallengeTracker.DBAccess.Repository;

namespace RainbowChallengeTracker.Interactions.Autocomplete
{
    public class AutoCompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            foreach (var challenge in ChallengeRepository.Challenges)
                choices.Add(new(challenge.Text, challenge.Text));
            return Task.FromResult(choices.Where(x =>
            {
                try
                {
                    if (ctx.OptionValue is null)
                        return true;
                    var optionString = ctx.OptionValue?.ToString();
                    if (string.IsNullOrWhiteSpace(optionString))
                        return true;
                    return x.Name.ToString().Contains(optionString);
                }
                catch
                {
                    return true;
                }
            }).Take(5));
        }
    }
}
