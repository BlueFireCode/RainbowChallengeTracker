using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using RainbowChallengeTracker.DBAccess.Repository;

namespace RainbowChallengeTracker.Interactions
{
    public class OwnerCommands : ApplicationCommandModule
    {
        [SlashCommand("ReloadCache", "Reload the client cache, if any changes have been made in the database manually.")]
        public async Task ReloadCache(InteractionContext ctx)
        {
            if (ctx.Member.Id != 387325006176059394)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "You cant do that!",
                    IsEphemeral = true
                });
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

            ChallengeRepository.ReloadCache();

            await ctx.FollowUpAsync(new() { Content = "Done!" });
        }

        [SlashCommand("setup", "Set up the bot for a server")]
        public async Task Setup(InteractionContext ctx,
            [Option("Category", "The category to use for challenge channels")] DiscordChannel category)
        {
            //check if user has perms
            if (!ctx.Member.Permissions.HasFlag(Permissions.Administrator))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "You have to be an admin of this guild to use this command!",
                    IsEphemeral = true
                });
                return;
            }

            //ack the interaction
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new()
            {
                IsEphemeral = true
            });

            //test access to category
            try
            {
                var channel = await ctx.Guild.CreateChannelAsync("test", ChannelType.Text, category);
                await channel.ModifyAsync(x => x.Name = "testing");
                await channel.AddOverwriteAsync(ctx.Member, allow: Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages | Permissions.ManageMessages);
                await channel.SendMessageAsync("test");
                await channel.DeleteAsync();
            }
            catch (UnauthorizedException)
            {
                await ctx.FollowUpAsync(new()
                {
                    Content = "I need permission to create, delete and modify channels in that category, aswell as to send messages in those channels!"
                });
                return;
            }

            //get guild from db
            var guild = GuildRepository.GetGuild(ctx.Guild.Id);

            if (guild is null)
            {
                await ctx.FollowUpAsync(new()
                {
                    Content = "Something has gone wrong in the database, try reinviting the bot!"
                });
                return;
            }
            //update guild in db
            guild.Category = category.Id;
            GuildRepository.UpdateGuild(guild);
            await ctx.FollowUpAsync(new() { Content = "Done!" });
        }
    }
}
