using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure;
using Mmcc.MemberBot.Infrastructure.Extensions;
using Mmcc.MemberBot.Infrastructure.Queries.Applications;

namespace Mmcc.MemberBot.Modules.Applications
{
    public class ViewApplicationsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ViewApplicationsModule> _logger;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _config;

        public ViewApplicationsModule(
            ILogger<ViewApplicationsModule> logger,
            IMediator mediator,
            DiscordSettings config
            )
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
        }

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
            var query = new GetApplicationById.Query
            {
                Id = applicationId
            };
            var app = await _mediator.Send(query);

            if (app is null)
            {
                var errorEmbed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage("Application with ID `{applicationId}` does not exist.")
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
            var pendingApps = (await _mediator.Send(new GetApplications.Query())).ToList();

            if (pendingApps.IsEmpty())
            {
                await Context.Channel.SendMessageAsync("There are no pending applications at the moment.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Pending applications")
                .WithColor(Color.Blue)
                .WithMmccLogo()
                .WithApplicationFields(pendingApps)
                .Build();

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("approved", RunMode = RunMode.Async)]
        [Summary("Shows last 10 approved applications")]
        public async Task ViewApprovedAsync()
        {
            var query = new GetLastApplicationsByStatus.Query
            {
                ApplicationStatus = ApplicationStatus.Approved
            };
            var approvedApps = (await _mediator.Send(query)).ToList();

            if (approvedApps.IsEmpty())
            {
                await Context.Channel.SendMessageAsync("No applications have been approved yet.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Approved applications")
                .WithDescription("Last 10 approved applications")
                .WithColor(Color.Green)
                .WithMmccLogo()
                .WithApplicationFields(approvedApps)
                .Build();

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("rejected", RunMode = RunMode.Async)]
        [Summary("Shows last 10 rejected applications")]
        public async Task ViewRejectedAsync()
        {
            var query = new GetLastApplicationsByStatus.Query
            {
                ApplicationStatus = ApplicationStatus.Rejected
            };
            var rejectedApps = (await _mediator.Send(query)).ToList();

            if (rejectedApps.IsEmpty())
            {
                await Context.Channel.SendMessageAsync("No applications have been rejected yet.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Rejected applications")
                .WithDescription("Last 10 rejected applications")
                .WithColor(Color.Red)
                .WithMmccLogo()
                .WithApplicationFields(rejectedApps)
                .Build();
            
            await Context.Channel.SendEmbedAsync(embed);
        }
    }
}