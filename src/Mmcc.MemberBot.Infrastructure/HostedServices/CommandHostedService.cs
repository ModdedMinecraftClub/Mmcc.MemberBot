using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Commands.Applications;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Infrastructure.HostedServices
{
    public class CommandHostedService : IHostedService
    {
        private readonly ILogger<CommandHostedService> _logger;
        private readonly DiscordSettings _config;
        private readonly IServiceProvider _provider;
        private readonly IBotService _botService;
        private readonly CommandService _commandService;
        
        public IServiceProvider Services { get; }

        public CommandHostedService(
            ILogger<CommandHostedService> logger,
            IServiceProvider provider,
            IBotService botService,
            CommandService commandService,
            DiscordSettings config,
            IServiceProvider services
            )
        {
            _logger = logger;
            _provider = provider;
            _botService = botService;
            _commandService = commandService;
            _config = config;
            Services = services;
        }
        
        private Task LogCommand(LogMessage arg)
        {
            var message = $"{arg.Source}: {arg.Message}";
            if (arg.Exception is null)
            {
                _logger.Log(arg.Severity.ToLogLevel(), message);
            }
            else
            {
                _logger.Log(arg.Severity.ToLogLevel(), arg.Exception, message);
            }

            return Task.CompletedTask;
        }
        
        private async Task BotMessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }
            
            if (!(message is SocketUserMessage userMessage))
            {
                return;
            }

            var argPos = 0;

            if (userMessage.Channel.Name.Equals(_config.ChannelNames.MemberApps) && message.Attachments.Count != 0)
            {
                var channel = userMessage.Channel;
                
                using (var scope = Services.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new CreateFromUserMsg.Command {UserMessage = userMessage});
                }
                
                await channel.SendMessageAsync(
                    "Your application has been submitted and you will be pinged once it has been processed.");
                return;
            }
            
            if (!userMessage.HasCharPrefix(_config.Prefix, ref argPos))
            {
                return;
            }

            _botService.ExecuteHandlerAsynchronously(
                handler: (client) =>
                {
                    var context = new SocketCommandContext(client, userMessage);
                    return _commandService.ExecuteAsync(context, argPos, _provider);
                },
                callback: async (result) =>
                {
                    if (result.IsSuccess)
                    {
                        return;
                    }

                    if (result.Error == CommandError.UnknownCommand)
                    {
                        return;
                    }

                    if (result.Error.HasValue)
                    {
                        var embed = new ErrorEmbedBuilder()
                            .WithErrorMessage($"{result.Error.Value}; {result.ErrorReason}")
                            .Build();
                        await message.Channel.SendEmbedAsync(embed);
                    }
                });
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandService.Log += LogCommand;
            _botService.DiscordClient.MessageReceived += BotMessageReceivedAsync;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _botService.DiscordClient.MessageReceived -= BotMessageReceivedAsync;
            _botService.DiscordClient.Log -= LogCommand;

            return Task.CompletedTask;
        }
    }
}