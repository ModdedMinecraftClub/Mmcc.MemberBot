using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ModdedMinecraftClub.MemberBot.Core;

namespace ModdedMinecraftClub.MemberBot.Bot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            
            _commands.CommandExecuted += CommandExecutedAsync;
            
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            // offset where the prefix ends
            var argPos = 0;

            if (message.Channel.Name.Equals("member-apps"))
            {
                var channel = message.Channel;
                
                await channel.SendMessageAsync("hello, you're in #member-apps");
            }
            else if (!message.HasCharPrefix(YamlConfiguration.Config.Discord.Prefix, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(_discord, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command not found => do nothing
            if (!command.IsSpecified)
            {
                return;
            }

            // if the command was successful => do nothing
            if (result.IsSuccess)
            {
                return;
            }

            // what to do if the command failed
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}