using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.ApplicationParser;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure;
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
        private readonly MmccApplicationSerializer _serializer;

        public ApproveApplicationsModule(
            ILogger<ApproveApplicationsModule> logger,
            IMediator mediator,
            DiscordSettings config, 
            MmccApplicationSerializer serializer
            )
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
            _serializer = serializer;
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-100)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync()
        {
            var alternativeUsages = new List<string>
            {
                $"{_config.Prefix}approve <applicationId> <serverPrefix> <ign>",
                $"{_config.Prefix}approve <applicationId> manual"
            };
            
            var embed = new IncorrectArgsEmbedBuilder()
                .WithStandardIncorrectArgsEmbedLayout()
                .WithUsageField($"{_config.Prefix}approve <applicationId>")
                .WithAlternativeUsages(alternativeUsages)
                .Build();
            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Approves a particular application. Attempts to obtain the username and server prefix automatically via parsing.")]
        [Priority(2)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId)
        {
            var app = await _mediator.Send(new GetApplicationById.Query {Id = applicationId});
            
            if (app is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Application with ID `{applicationId}` does not exist.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }
            
            // parse;
            DiscordApplication deserializedMsgContent;
            try
            {
                deserializedMsgContent = _serializer.Deserialize<DiscordApplication>(app.MessageContent);
            }
            catch (Exception e)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage("Could not obtain the username and server prefix from the message automatically via parsing.")
                    .WithException(e)
                    .WithHowToDealWithThisErrorField($"Please promote the player via `{_config.Prefix}approve <applicationId> <serverPrefix> <ign>`.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }

            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            var memberRole = Context.Guild.Roles.FindMemberRoleContains(deserializedMsgContent.Server);

            if (memberRole is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Could not figure out which Discord role belongs to the server given by the user.\nGiven server: {deserializedMsgContent.Server}")
                    .WithHowToDealWithThisErrorField($"Please promote the player via `{_config.Prefix}approve <applicationId> <serverPrefix> <ign>`.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }

            var userToPromote = await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, app.AuthorDiscordId);
            var roleName = memberRole.Name;
            var serverPrefix = roleName.Substring(roleName.IndexOf('[') + 1, roleName.LastIndexOf(']') - 1);
            
            if (userToPromote is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Cannot find a user with ID `{app.AuthorDiscordId}`.")
                    .WithHowToDealWithThisErrorField($"Please promote manually via `{_config.Prefix}approve <applicationId> manual` or reject the application.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }
            
            // promote;
            var command = new Promote.Command
            {
                ServerPrefix = serverPrefix,
                Ign = deserializedMsgContent.Ign,
                ApplicationId = applicationId,
                MemberRole = memberRole,
                UserToPromote = userToPromote
            };

            try
            {
                await _mediator.Send(command);
            }
            catch (IOException ioException)
            {
                var ioExEmbed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage("Could not send the message to Polychat. Is Polychat online?")
                    .WithException(ioException)
                    .Build();
                await Context.Channel.SendEmbedAsync(ioExEmbed);
                return;
            }

            // notify;
            await NotifyAboutApprovedApp(app, membersChannel);
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Approves a particular application")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string serverPrefix, string ign)
        {
            var app = await _mediator.Send(new GetApplicationById.Query {Id = applicationId});
            
            // validate;
            if (app is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Application with ID `{applicationId}` does not exist.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }
            
            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            var memberRole = Context.Guild.Roles.FindMemberRole(serverPrefix);

            if (memberRole is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Prefix `{serverPrefix}` does not exist.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
                return;
            }

            var userToPromote = await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, app.AuthorDiscordId);
            
            if (userToPromote is null)
            {
                var embed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage($"Cannot find a user with ID `{app.AuthorDiscordId}`.")
                    .WithHowToDealWithThisErrorField($"Please promote manually via `{_config.Prefix}promote <applicationId> manual` or reject the application.")
                    .Build();
                await Context.Channel.SendEmbedAsync(embed);
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
            try
            {
                await _mediator.Send(command);
            }
            catch (IOException ioException)
            {
                var ioExEmbed = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage("Could not send the message to Polychat. Is Polychat online?")
                    .WithException(ioException)
                    .Build();
                await Context.Channel.SendEmbedAsync(ioExEmbed);
                return;
            }
            
            // notify;
            await NotifyAboutApprovedApp(app, membersChannel);
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Summary("Force mark an application as approved (player will not be promoted automatically, you will have to promote them manually but the application will be marked as approved and will be removed from the pending list)")]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ApproveAsync(int applicationId, string manual)
        {
            if (!manual.Equals("manual"))
            {
                await ApproveAsync();
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

        private async Task NotifyAboutApprovedApp(Application app, ISocketMessageChannel membersChannel)
        {
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
                $":white_check_mark: **Approved** application with ID: `{app.AppId}`");
        }
    }
}