﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using RainbowChallengeTracker.DBAccess.Models;
using RainbowChallengeTracker.DBAccess.Repository;
using RainbowChallengeTracker.Interactions.Autocomplete;

namespace RainbowChallengeTracker.Interactions
{
    public class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("AddChallenge", "Add a challenge for yourself")]
        public async Task AddChallenge(
            InteractionContext ctx,
            [Option("Text", "The challenge text", true)]
            [Autocomplete(typeof(AutoCompleteProvider))]
            long id,
            [Option("AlreadyCompleted", "The amount of Kills/Drone kills/etc.. you have already done")]
            long? alreadyCompleted = null
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

            var guild = GuildRepository.GetGuild(ctx.Guild.Id);
            if (guild is null || guild.Category is null)
            {
                await ctx.FollowUpAsync(new()
                {
                    Content = "This server is not yet set up to use this bot! Ask your admins to set it up."
                });
                return;
            }

            if (!ChallengeRepository.Challenges.Any(x => x.ID == id))
            {
                await ctx.FollowUpAsync(new() { Content = "Could not find challenge, please create it first, using CreateChallenge!" });
                return;
            }

            var category = ctx.Guild.GetChannel((ulong)guild.Category);
            if (category.Children.Any(x => x.Topic is not null && x.Topic.StartsWith(ctx.Member.Id.ToString())))
            {
#pragma warning disable CS8604
                await SendMessageAsync(category.Children.First(x => x.Topic.StartsWith(ctx.Member.Id.ToString())),
                    ChallengeRepository.Challenges.Find(x => x.ID == (int)id),
                    (int?)alreadyCompleted);
#pragma warning restore CS8604
                await ctx.FollowUpAsync(new() { Content = "Done!" });
                return;
            }

            var channel = await ctx.Guild.CreateChannelAsync(ctx.Member.DisplayName,
                ChannelType.Text,
                category,
                ctx.Member.Id.ToString() + " You can safely edit this channel topic, as long as you leave this id at the beginning of it.\n" +
                    "The channel's name can also safely be changed.");
            await channel.AddOverwriteAsync(ctx.Member, allow:
                Permissions.AccessChannels |
                Permissions.ManageChannels |
                Permissions.SendMessages |
                Permissions.ManageMessages);
#pragma warning disable CS8604
            await SendMessageAsync(channel,
                ChallengeRepository.Challenges.Find(x => x.ID == (int)id),
                (int?)alreadyCompleted);
#pragma warning restore CS8604
            await ctx.FollowUpAsync(new() { Content = $"Done! {channel.Mention}" });
        }

        [SlashCommand("CreateChallenge", "Create a new challenge for yourself")]
        public async Task CreateChallenge
        (
            InteractionContext ctx,
            [Option("Text", "The challenge text")]
            string text,
            [Option("Amount", "The amount of Kills/Drone kills/etc.. the challenge asks for in total")]
            long amount
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

            if (ChallengeRepository.Challenges.Any(x => x.Text == text))
            {
                await ctx.FollowUpAsync(new() { Content = "This challenge already exists!" });
                return;
            }
            
            ChallengeRepository.CreateChallenge(new()
            {
                Text = text,
                Amount = (int)amount
            });

            await ctx.FollowUpAsync(new() { Content = $"Done!" });
        }

        private async Task SendMessageAsync(DiscordChannel channel, ChallengeModel challenge, int? alreadyCompleted)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Description = challenge.Text,
                Color = DiscordColor.Red
            };
            embed.AddField("Total amount", challenge.Amount.ToString());
            embed.AddField("Amount done", (alreadyCompleted is null ? 0 : alreadyCompleted).ToString());

            var message = new DiscordMessageBuilder()
            {
                Embed = embed
            };
            message.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "increment", "+1"),
                new DiscordButtonComponent(ButtonStyle.Danger, "decrement", "-1")
            });
            message.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "done", "Completed", emoji: new DiscordComponentEmoji("✔️")));
            await message.SendAsync(channel);
        }
    }
}
