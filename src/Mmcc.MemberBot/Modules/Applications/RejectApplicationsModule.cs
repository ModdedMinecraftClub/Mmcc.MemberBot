using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure;
using Mmcc.MemberBot.Infrastructure.Commands.Applications;
using Mmcc.MemberBot.Infrastructure.Extensions;
using Mmcc.MemberBot.Infrastructure.Queries.Applications;

namespace Mmcc.MemberBot.Modules.Applications
{
    public class RejectApplicationsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<RejectApplicationsModule> _logger;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _config;

        public RejectApplicationsModule(ILogger<RejectApplicationsModule> logger, IMediator mediator, DiscordSettings config)
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
        }

        [Command("reject", RunMode = RunMode.Async)]
        [Priority(-100)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task RejectAsync()
        {
            var embed = new IncorrectArgsEmbedBuilder()
                .WithStandardIncorrectArgsEmbedLayout()
                .WithUsageField($"{_config.Prefix}reject <applicationId> <reason>")
                .Build();
            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("reject", RunMode = RunMode.Async)]
        [Summary("Rejects a particular application")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task RejectAsync(int applicationId, [Remainder]string reason)
        {
            // validate;
            var app = await _mediator.Send(new GetApplicationById.Query {Id = applicationId});

            if (app is null)
            {
                var errorEmbed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Application with ID `{applicationId}` does not exist.")
                    .Build();
                await Context.Channel.SendEmbedAsync(errorEmbed);
                return;
            }
            
            // reject;
            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            var command = new ChangeApplicationStatus.Command
            {
                ApplicationId = applicationId,
                ApplicationStatus = ApplicationStatus.Rejected
            };

            await _mediator.Send(command);
            
            // notify;
            
            // notify the user that their app has been rejected;
            var embed = new EmbedBuilder()
                .WithTitle("Application rejected")
                .WithDescription("Unfortunately, your application has been rejected.")
                .WithApplicationStatusColour(ApplicationStatus.Rejected)
                .WithMmccLogo()
                .AddField("Reason", reason)
                .AddField("Rejected by", Context.Message.Author)
                .Build();
            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}>", false, embed);
            
            // notify the staff member that the app has been successfully rejected;
            await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
        }
    }
}