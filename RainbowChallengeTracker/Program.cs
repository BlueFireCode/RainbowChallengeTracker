using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using RainbowChallengeTracker.DBAccess.Repository;
using RainbowChallengeTracker.Interactions;

namespace RainbowChallengeTracker
{
    public class Program
    {
        public static void Main(string[] _)
        {
#if DEBUG
            foreach (var line in File.ReadAllLines("settings.env"))
                Environment.SetEnvironmentVariable(line[..line.IndexOf('=')], line[(line.IndexOf('=') + 1)..]);
#endif
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            DiscordConfiguration config = new()
            {
                Token = Environment.GetEnvironmentVariable("TOKEN"),
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                //Intents = DiscordIntents.Guilds,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug
#endif
            };

            DiscordClient client = new(config);

            //Register client events
            client.Ready += Client_Ready;
            client.ClientErrored += Client_ClientErrored;
            client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            client.GuildCreated += Client_GuildCreated;
            client.GuildDeleted += Client_GuildDeleted;

            var slashCommands = client.UseSlashCommands();

            //register interactions
            slashCommands.RegisterCommands<OwnerCommands>(512370308532142091);
            slashCommands.RegisterCommands<UserCommands>(512370308532142091);

            //register interaction events
            slashCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;

            await client.ConnectAsync();

            await Task.Delay(-1);
        }

        private static Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            GuildRepository.DeleteGuild(e.Guild.Id);
            return Task.CompletedTask;
        }

        private static Task Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            GuildRepository.CreateGuild(new() { Id = e.Guild.Id });
            return Task.CompletedTask;
        }

        private static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            var messageBuilder = new DiscordMessageBuilder();
            if (e.Id == "increment")
            {
                var completed = int.Parse(e.Message.Embeds[0].Fields[1].Value);
                if (completed + 1 >= int.Parse(e.Message.Embeds[0].Fields[0].Value))
                {
                    var rows = e.Message.Components.First();
                    var components = rows.Components.ToList();
                    components.ForEach(x => ((DiscordButtonComponent)x).Disable());

                    messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Completed",
                        Description = $"~~{e.Message.Embeds[0].Description}~~",
                        Color = DiscordColor.Green
                    }).AddComponents(components);
                }
                else
                {
                    e.Message.Embeds[0].Fields[1].Value = (completed + 1).ToString();
                    var rows = e.Message.Components.First();
                    var components = rows.Components.ToList();
                    messageBuilder.AddEmbed(e.Message.Embeds[0]).AddComponents(components);
                }
            }
            else if (e.Id == "decrement")
            {
                var completed = int.Parse(e.Message.Embeds[0].Fields[1].Value);
                if (completed - 1 <= 0)
                    return;

                e.Message.Embeds[0].Fields[1].Value = (completed - 1).ToString();
                var rows = e.Message.Components.First();
                var components = rows.Components.ToList();
                messageBuilder.AddEmbed(e.Message.Embeds[0]).AddComponents(components);
            }
            else if (e.Id == "done")
            {
                var rows = e.Message.Components.First();
                var components = rows.Components.ToList();
                components.ForEach(x => ((DiscordButtonComponent)x).Disable());

                messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Completed",
                    Description = $"~~{e.Message.Embeds[0].Description}~~",
                    Color = DiscordColor.Green
                }).AddComponents(components);
            }
            await e.Message.ModifyAsync(messageBuilder);
        }

        private static Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            //todo: add proper logging
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(e.Exception.Message);
            Console.WriteLine(e.Exception.StackTrace);
            return Task.CompletedTask;
        }

        private static Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            //todo: add proper logging
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(e.Exception.Message);
            Console.WriteLine(e.Exception.StackTrace);
            return Task.CompletedTask;
        }

        private static async Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            DiscordActivity activity = new("Rainbow Six Siege", ActivityType.Playing);
            await sender.UpdateStatusAsync(activity);
        }
    }
}