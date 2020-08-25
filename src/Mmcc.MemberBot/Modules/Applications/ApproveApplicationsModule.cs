using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Commands.Applications;
using Mmcc.MemberBot.Infrastructure.Extensions;
using Mmcc.MemberBot.Infrastructure.Queries.Applications;

namespace Mmcc.MemberBot.Modules.Applications
{
    public class ApproveApplicationsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ApproveApplicationsModule> _logger;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _config;

        public ApproveApplicationsModule(
            ILogger<ApproveApplicationsModule> logger,
            IMediator mediator,
            DiscordSettings config
            )
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-100)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync()
        {
            const string msg = ":x: Incorrect arguments.\nUsage: `approve <application id> <server prefix> <ign>`\nAlternative usage: `approve <application id> manual`";
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Approves a particular application")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string serverPrefix, string ign)
        {
            var app = await _mediator.Send(new GetApplicationById.Query{Id = applicationId});
            
            // validate;
            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                return;
            }
            
            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            var memberRole = Context.Guild.Roles.FindMemberRole(serverPrefix);

            if (memberRole is null)
            {
                await Context.Channel.SendMessageAsync($":x: Prefix `{serverPrefix}` does not exist.");
                return;
            }

            var userToPromote = Context.Guild.Users.FindMemberAppAuthor(app.AuthorDiscordId);
            
            if (userToPromote is null)
            {
                await Context.Channel.SendMessageAsync(
                    $":x: Cannot find a user with ID `{app.AuthorDiscordId}`. Please promote manually or reject the application.");
                return;
            }
            
            // promote;
            var command = new Promote.Command
            {
                ServerPrefix = serverPrefix,
                Ign = ign,
                ApplicationId = applicationId,
                MemberRole = memberRole,
                UserToPromote = userToPromote
            };
            await _mediator.Send(command);
            
            // notify;
            
            // notify the user that their app has been approved;
            var userNotificationEmbed = new EmbedBuilder()
                .WithTitle("Application approved")
                .WithDescription($"Congratulations, your application has been approved.")
                .WithApplicationStatusColour(ApplicationStatus.Approved)
                .WithMmccLogo()
                .AddField("Approved by", Context.Message.Author)
                .Build();
            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}>", false, userNotificationEmbed);
            
            // notify the staff member that the app has been successfully approved;
            await Context.Channel.SendMessageAsync(
                $":white_check_mark: **Approved** application with ID: `{applicationId}`");
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Force mark an application as approved (player will not be promoted automatically, you will have to promote them manually but the application will be marked as approved and will be removed from the pending list)")]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string manual)
        {
            if (!manual.Equals("manual"))
            {
                await Context.Channel.SendMessageAsync(":x: Incorrect arguments.\nUsage: `approve <application id> <server prefix> <ign>`\nAlternative usage: `approve <application id> manual`");
                return;
            }
            
            var command = new ChangeApplicationStatus.Command
            {
                ApplicationId = applicationId,
                ApplicationStatus = ApplicationStatus.Approved
            };
            
            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            
            await _mediator.Send(command);
            await Context.Channel.SendMessageAsync($":white_check_mark: **Marked** application with ID `{applicationId}` as approved but the player still has to be promoted manually.\n\n" +
                                                   "**Remember to:**\n" +
                                                   ":one: Promote the player in-game via Polychat/MC\n" +
                                                   ":two: Give the player the appropriate Discord role\n" +
                                                   $":three: Let the player know in <#{membersChannel.Id}> that they've been promoted manually");
        }
    }
}