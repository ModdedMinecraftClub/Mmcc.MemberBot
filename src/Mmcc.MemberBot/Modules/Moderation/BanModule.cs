using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure;
using Mmcc.MemberBot.Infrastructure.Commands.Moderation;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Modules.Moderation
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<BanModule> _logger;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _config;
        private readonly IModerationService _moderationService;

        public BanModule(ILogger<BanModule> logger, IMediator mediator, DiscordSettings config, IModerationService moderationService)
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
            _moderationService = moderationService;
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Priority(-100)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync()
        {
            var alternativeUsages = new List<string>
            {
                $"{_config.Prefix}ban <discordUser> <duration> <reason>",
                $"{_config.Prefix}ban <ign> <duration> <reason>"
            };

            var embed = new IncorrectArgsEmbedBuilder()
                .WithStandardIncorrectArgsEmbedLayout()
                .WithUsageField($"{_config.Prefix}ban <discordUser> <ign> <duration> <reason>")
                .WithAlternativeUsages(alternativeUsages)
                .Build();
            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans a certain user. Both on Discord and in-game.")]
        [Priority(3)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser user, string ign, string duration, [Remainder]string reason)
        {
            // parse duration
            var timeSpan = await _moderationService.HandleDuration(Context.Channel, duration);
            if (timeSpan is null) return;
            
            // create expiry date
            var expiryDate = DateTime.UtcNow + timeSpan.Value;

            // create moderation event
            var mEvent = new ModerationEvent
            {
                Type = ModerationEventType.Ban,
                Active = true,
                ExpiryDateUtc = expiryDate,
                DiscordId = user.Id,
                Ign = ign,
                Reason = reason
            };
            
            // ban in-game
            await _moderationService.SendInGameCommand(mEvent);
            
            // notify the user
            await _moderationService.SendDmNotification(user, mEvent);
            
            // ban from Discord
            await Context.Guild.AddBanAsync(user, 0, reason);
            
            // insert into DB
            var command = new InsertNewModerationEvent.Command{ModerationEvent = mEvent};
            await _mediator.Send(command);
            
            // notify staff
            await _moderationService.SendSuccessStaffNotification(Context.Channel, mEvent);
        }
    }
}