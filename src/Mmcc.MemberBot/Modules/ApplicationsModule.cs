using System.Linq;
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

namespace Mmcc.MemberBot.Modules
{
    public class ApplicationsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ApplicationsModule> _logger;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _config;

        public ApplicationsModule(
            ILogger<ApplicationsModule> logger,
            IMediator mediator,
            DiscordSettings config
        )
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
        }
        
        #region View
        
        [Command("view", RunMode = RunMode.Async)]
        [Priority(-1)]
        public async Task ViewAsync()
        {
            var embed = new IncorrectArgsEmbedBuilder()
                .WithStandardIncorrectArgsEmbedLayout()
                .WithUsageField($"{_config.Prefix}view <applicationId>")
                .Build();
            await Context.Channel.SendEmbedAsync(embed);
        }
        
        [Command("view", RunMode = RunMode.Async)]
        [Summary("Shows a particular application")]
        [Priority(1)]
        public async Task ViewAsync(int applicationId)
        {
            var query = new GetById.Query {Id = applicationId};
            var app = await _mediator.Send(query);

            if (app is null)
            {
                var errorEmbed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Application with ID `{applicationId}` does not exist.")
                    .Build();
                await Context.Channel.SendEmbedAsync(errorEmbed);
                return;
            }

            var embed = new EmbedBuilder()
                .AddField($"{app.AppStatus.ToString().ToUpper()}: Application by {app.AuthorName}",
                    $"Author's Discord ID: {app.AuthorDiscordId}\nApplication ID: {app.AppId}")
                .AddField("Provided details",
                    string.IsNullOrWhiteSpace(app.MessageContent)
                        ? "*Player did not provide any details.*"
                        : app.MessageContent)
                .AddField("Link to original message", app.MessageUrl)
                .WithThumbnailUrl(app.ImageUrl)
                .WithFooter($"Applied at {app.AppTime}")
                .WithApplicationStatusColour(app.AppStatus)
                .Build();

            await Context.Channel.SendEmbedAsync(embed);
        }
        
        [Command("pending", RunMode = RunMode.Async)]
        [Summary("Shows currently pending applications")]
        public async Task ViewPendingAsync()
        {
            await SendApplicationsList(ApplicationStatus.Pending, null);
        }

        [Command("rejected", RunMode = RunMode.Async)]
        [Summary("Shows last 10 rejected applications")]
        public async Task ViewRejectedAsync()
        {
            await SendApplicationsList(ApplicationStatus.Rejected, 10);
        }
        
        [Command("approved", RunMode = RunMode.Async)]
        [Summary("Shows last 10 rejected applications")]
        public async Task ViewApprovedAsync()
        {
            await SendApplicationsList(ApplicationStatus.Approved, 10);
        }

        #endregion
        
        #region Approve
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-100)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync()
        {
            var embed = new IncorrectArgsEmbedBuilder()
                .WithStandardIncorrectArgsEmbedLayout()
                .WithUsageField($"{_config.Prefix}approve <applicationId> <serverPrefix> <ign>")
                .WithAlternativeUsage($"{_config.Prefix}approve <applicationId> manual")
                .Build();
            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Approves a particular application")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string serverPrefix, string ign)
        {
            var cmd = new ApproveAutomatically.Command
            {
                RestClient = Context.Client.Rest,
                Guild = Context.Guild,
                Ign = ign,
                ApplicationId = applicationId,
                ServerPrefix = serverPrefix
            };
            var cmdResult = await _mediator.Send(cmd);

            if (cmdResult.Succeeded)
            {
                var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
                var userNotificationEmbed = new EmbedBuilder()
                    .WithTitle("Application approved")
                    .WithDescription("Congratulations, your application has been approved.")
                    .WithApplicationStatusColour(ApplicationStatus.Approved)
                    .WithMmccLogo()
                    .AddField("Approved by", Context.Message.Author)
                    .Build();
                var staffNotificationEmbed = new EmbedBuilder()
                    .WithTitle(":white_check_mark: Approved the application successfully!")
                    .WithDescription($"Application with ID `{applicationId}` has been *approved*.")
                    .WithMmccLogo()
                    .WithColor(Color.Green)
                    .Build();
                
                await membersChannel.SendMessageAsync($"<@{cmdResult.Payload!.AuthorDiscordId}>", false, userNotificationEmbed);
                await Context.Channel.SendEmbedAsync(staffNotificationEmbed);
            }
            else
            {
                await Context.Channel.SendEmbedAsync(cmdResult.ToErrorEmbed());
            }
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Summary(
            "Force mark an application as approved (player will not be promoted automatically, you will have to promote them manually but the application will be marked as approved and will be removed from the pending list)")]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string manual)
        {
            if (!manual.Equals("manual"))
            {
                await ApproveAsync();
                return;
            }

            var cmd = new ChangeStatusSimple.Command
                {ApplicationId = applicationId, NewStatus = ApplicationStatus.Approved};
            var cmdResult = await _mediator.Send(cmd);

            if (cmdResult.Succeeded)
            {
                var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
                await Context.Channel.SendMessageAsync($":white_check_mark: **Marked** application with ID `{applicationId}` as approved but the player still has to be promoted manually.\n\n" +
                                                       "**Remember to:**\n" +
                                                       ":one: Promote the player in-game via Polychat/MC\n" +
                                                       ":two: Give the player the appropriate Discord role\n" +
                                                       $":three: Let the player know in <#{membersChannel.Id}> that they've been promoted manually");
            }
            else
            {
                await Context.Channel.SendEmbedAsync(cmdResult.ToErrorEmbed());
            }
        }
        
        #endregion

        #region Reject

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
        public async Task RejectAsync(int applicationId, [Remainder] string reason)
        {
            var cmd = new ChangeStatusSimple.Command
                {ApplicationId = applicationId, NewStatus = ApplicationStatus.Rejected};
            var cmdResult = await _mediator.Send(cmd);

            if (cmdResult.Succeeded)
            {
                var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
                var userNotificationEmbed = new EmbedBuilder()
                    .WithTitle("Application rejected")
                    .WithDescription("Unfortunately, your application has been rejected.")
                    .WithApplicationStatusColour(ApplicationStatus.Rejected)
                    .WithMmccLogo()
                    .AddField("Reason", reason)
                    .AddField("Rejected by", Context.Message.Author)
                    .Build();
                var staffNotificationEmbed = new EmbedBuilder()
                    .WithTitle(":white_check_mark: Rejected the application successfully!")
                    .WithDescription($"Application with ID `{applicationId}` has been *rejected*.")
                    .WithMmccLogo()
                    .WithColor(Color.Green)
                    .Build();
                
                await membersChannel.SendMessageAsync($"<@{cmdResult.Payload!.AuthorDiscordId}>", false, userNotificationEmbed);
                await Context.Channel.SendEmbedAsync(staffNotificationEmbed);
            }
            else
            {
                await Context.Channel.SendEmbedAsync(cmdResult.ToErrorEmbed());
            }
        }

        #endregion

        private async Task SendApplicationsList(ApplicationStatus status, int? limit)
        {
            var pendingApps = await _mediator.Send(new GetByStatus.Query
                {Status = status, Limit = limit});
            var statusText = status.ToString().ToLower();

            if (pendingApps.IsEmpty())
            {
                await Context.Channel.SendMessageAsync($"There are no {statusText} applications at the moment.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle(limit is null
                    ? $"{statusText.First().ToString().ToUpper() + statusText.Substring(1)} applications"
                    : $"Last {limit} {statusText} applications")
                .WithColor(Color.Blue)
                .WithMmccLogo()
                .WithApplicationFields(pendingApps)
                .Build();

            await Context.Channel.SendEmbedAsync(embed);
        }
    }
}